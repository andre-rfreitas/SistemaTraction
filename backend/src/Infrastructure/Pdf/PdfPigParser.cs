using System.Text.RegularExpressions;
using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace SistemaTraction.Infrastructure.Pdf;

/// <summary>
/// Parser PDF com duas estratégias:
/// 1) Varredura de texto completo — melhor para PDFs gerados por HTML (UpSeller, etc.)
/// 2) Posicionamento de palavras — fallback para ERPs genéricos
/// </summary>
public class PdfPigParser : IPdfParser
{
    // SKU: ao menos 2 partes separadas por hífen, tudo maiúsculo/dígito
    // Captura mesmo no meio de uma linha, com lookbehind/lookahead para evitar falsos positivos
    private static readonly Regex SkuInLineRegex = new(
        @"(?<![A-Z0-9\-])[A-Z][A-Z0-9]{0,7}(?:-[A-Z0-9]{1,8}){2,4}(?![A-Z0-9\-])",
        RegexOptions.Compiled);

    // Tamanhos usados na estratégia legada
    private static readonly HashSet<string> KnownSizes =
        new(StringComparer.OrdinalIgnoreCase) { "P", "M", "G", "GG", "G1", "XG", "XGG", "PP" };

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);

            // Estratégia 1: varredura de page.Text (UpSeller e PDFs de HTML)
            var items = ParseByTextScan(doc);
            if (items.Count > 0)
                return new ParseResult(true, items, null);

            // Estratégia 2: posicionamento de palavras (ERPs genéricos)
            var fallback = ParseByWordPositions(doc);
            if (fallback.Count > 0)
                return new ParseResult(true, fallback, null);

            return new ParseResult(false, [],
                "Nenhum item encontrado. Verifique se o PDF é uma lista de separação válida e " +
                "se os Códigos SKU estão configurados na aba 'Config. SKU'.");
        }
        catch (Exception ex)
        {
            return new ParseResult(false, [], $"Erro ao ler o PDF: {ex.Message}");
        }
    }

    // ── Estratégia 1: varredura de texto ─────────────────────────────────────────
    // Extrai o texto completo de cada página como string, normaliza hífens e
    // varre linha a linha procurando padrão de SKU.
    // A quantidade pode estar na mesma linha (após o SKU) ou na linha seguinte.
    // Default: 1 por linha (padrão UpSeller onde cada pedido = 1 unidade).

    private static List<ParsedItem> ParseByTextScan(PdfDocument doc)
    {
        var allLines = new List<string>();

        foreach (var page in doc.GetPages())
        {
            var text = NormalizeHyphens(page.Text ?? "");
            foreach (var raw in text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = raw.Trim();
                if (line.Length >= 3) allLines.Add(line);
            }
        }

        var accumulated = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < allLines.Count; i++)
        {
            var line = allLines[i];
            var match = SkuInLineRegex.Match(line);
            if (!match.Success) continue;

            var sku = match.Value;
            var qty = 0;

            // Quantidade após o SKU na mesma linha (ex: "REG-REDR-REDGG 1")
            var rest = line[(match.Index + match.Length)..].Trim();
            if (rest.Length > 0 && rest.Length <= 5 &&
                int.TryParse(rest, out var q1) && q1 > 0 && q1 < 9999)
            {
                qty = q1;
            }

            // Quantidade na próxima linha (ex: "REG-REDR-RED-G" + "1")
            if (qty == 0 && i + 1 < allLines.Count)
            {
                var next = allLines[i + 1];
                if (next.Length <= 4 &&
                    int.TryParse(next, out var q2) && q2 > 0 && q2 < 9999)
                {
                    qty = q2;
                    i++; // consome a linha de quantidade
                }
            }

            if (qty == 0) qty = 1; // UpSeller: cada linha = 1 unidade

            accumulated[sku] = accumulated.GetValueOrDefault(sku) + qty;
        }

        return accumulated
            .OrderBy(kv => kv.Key)
            .Select((kv, idx) => new ParsedItem(kv.Key, "", "", kv.Value, idx))
            .ToList();
    }

    // ── Estratégia 2: posicionamento de palavras ──────────────────────────────────
    // Para ERPs que listam Cor / Tamanho / Quantidade em colunas separadas.

    private static List<ParsedItem> ParseByWordPositions(PdfDocument doc)
    {
        const double rowTolerance = 4.0;

        var allWords = new List<(double Y, double X, string Text)>();
        foreach (var page in doc.GetPages())
        {
            var height = page.Height;
            foreach (var word in page.GetWords())
                allWords.Add((height - word.BoundingBox.Bottom, word.BoundingBox.Left,
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
                if (int.TryParse(row[j], out var q) && q > 0 && q < 9999) { qty = q; break; }
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

    /// Normaliza variantes de hífen para ASCII hífen padrão.
    private static string NormalizeHyphens(string text) =>
        Regex.Replace(text, @"[­‑‒–—−﹣－]", "-");

    private static bool IsHeaderWord(string t) =>
        t.Equals("Cor",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Tamanho",    StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Qtd",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("SKU",        StringComparison.OrdinalIgnoreCase) ||
        t.Equals("Quantidade", StringComparison.OrdinalIgnoreCase);
}
