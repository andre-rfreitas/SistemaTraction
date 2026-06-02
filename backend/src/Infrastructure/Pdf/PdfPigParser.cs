using System.Text;
using System.Text.RegularExpressions;
using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace SistemaTraction.Infrastructure.Pdf;

/// <summary>
/// Parser PDF com duas estratégias:
/// 1) Varredura completa de texto — melhor para HTML-to-PDF (UpSeller, etc.)
/// 2) Posicionamento de palavras — fallback para ERPs genéricos
/// </summary>
public class PdfPigParser : IPdfParser
{
    // Padrão SKU: ao menos 2 partes maiúsculas/dígitos separadas por hífen
    // Lookbehind/lookahead garantem que não é parte de um token maior
    private static readonly Regex SkuRegex = new(
        @"(?<![A-Z0-9\-])[A-Z][A-Z0-9]{0,7}(?:-[A-Z0-9]{1,8}){2,4}(?![A-Z0-9\-])",
        RegexOptions.Compiled);

    // Número imediatamente após o SKU (mesma linha ou próxima, após espaços/newlines)
    private static readonly Regex QtyAfterSku = new(
        @"^[\s\r\n]*(\d{1,4})\b",
        RegexOptions.Compiled);

    private static readonly HashSet<string> KnownSizes =
        new(StringComparer.OrdinalIgnoreCase) { "P", "M", "G", "GG", "G1", "XG", "XGG", "PP" };

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);

            // Estratégia 1: varredura de texto completo (UpSeller e PDFs de HTML)
            var items = ParseByFullTextScan(doc);
            if (items.Count > 0)
                return new ParseResult(true, items, null);

            // Estratégia 2: posicionamento de palavras (fallback)
            var fallback = ParseByWordPositions(doc);
            if (fallback.Count > 0)
                return new ParseResult(true, fallback, null);

            return new ParseResult(false, [],
                "Nenhum item encontrado. Verifique se o PDF é uma lista de separação válida " +
                "e se os Códigos SKU estão configurados na aba 'Config. SKU'.");
        }
        catch (Exception ex)
        {
            return new ParseResult(false, [], $"Erro ao ler o PDF: {ex.Message}");
        }
    }

    // ── Estratégia 1: varredura de ALL matches no texto completo ──────────────────
    // Extrai todo o texto de todas as páginas como uma única string,
    // localiza TODAS as ocorrências de padrão SKU e tenta encontrar a
    // quantidade logo após cada uma (mesma linha ou linha seguinte).
    // Padrão UpSeller: cada row = 1 unidade, mesmo SKU repetido N vezes = qty N.

    private static List<ParsedItem> ParseByFullTextScan(PdfDocument doc)
    {
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages())
            sb.Append(NormalizeHyphens(page.Text ?? "")).Append('\n');

        var fullText = sb.ToString();
        var accumulated = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (Match skuMatch in SkuRegex.Matches(fullText))
        {
            var sku = skuMatch.Value;
            var qty = ExtractQtyAfter(fullText, skuMatch.Index + skuMatch.Length);
            accumulated[sku] = accumulated.GetValueOrDefault(sku) + qty;
        }

        return accumulated
            .Select((kv, idx) => new ParsedItem(kv.Key, "", "", kv.Value, idx))
            .ToList();
    }

    // Encontra o primeiro número nos ~20 chars seguintes ao SKU (skip whitespace/newlines).
    // Se não encontrar, retorna 1 (padrão UpSeller: cada linha = 1 unidade).
    private static int ExtractQtyAfter(string text, int startIndex)
    {
        var end = Math.Min(startIndex + 20, text.Length);
        var segment = text[startIndex..end];

        var m = QtyAfterSku.Match(segment);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var q) && q > 0 && q < 9_999)
            return q;

        return 1;
    }

    // ── Estratégia 2: posicionamento de palavras ──────────────────────────────────
    // Para ERPs que expõem Cor / Tamanho / Quantidade em colunas explícitas.

    private static List<ParsedItem> ParseByWordPositions(PdfDocument doc)
    {
        const double rowTolerance = 4.0;
        var allWords = new List<(double Y, double X, string Text)>();

        foreach (var page in doc.GetPages())
        {
            var height = page.Height;
            foreach (var word in page.GetWords())
                allWords.Add((height - word.BoundingBox.Bottom,
                              word.BoundingBox.Left,
                              NormalizeHyphens(word.Text)));
        }

        var rows = allWords
            .GroupBy(w => Math.Round(w.Y / rowTolerance) * rowTolerance)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(w => w.X).Select(w => w.Text).ToList())
            .Where(r => r.Count > 0)
            .ToList();

        var items = new List<ParsedItem>();
        var sortOrder = 0;

        foreach (var row in rows)
        {
            var sizeIdx = row.FindIndex(t => KnownSizes.Contains(t));
            if (sizeIdx < 0) continue;

            var qty = 0;
            for (var j = sizeIdx + 1; j < row.Count; j++)
            {
                if (int.TryParse(row[j], out var q) && q > 0 && q < 9_999) { qty = q; break; }
            }
            if (qty == 0) continue;

            var size  = row[sizeIdx];
            var color = sizeIdx > 0 && !int.TryParse(row[sizeIdx - 1], out _) ? row[sizeIdx - 1] : null;
            if (color == null || IsHeaderWord(color)) continue;

            var sku = sizeIdx > 1 ? string.Join(" ", row.Take(sizeIdx - 1)).Trim() : "";
            items.Add(new ParsedItem(sku, color.Trim(), size, qty, sortOrder++));
        }

        return items;
    }

    // ── Utilitários ───────────────────────────────────────────────────────────────

    private static string NormalizeHyphens(string text) =>
        Regex.Replace(text, @"[­‑‒–—−﹣－]", "-");

    private static bool IsHeaderWord(string t) =>
        t.Equals("Cor",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Tamanho",    StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Qtd",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("SKU",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Quantidade", StringComparison.OrdinalIgnoreCase);
}
