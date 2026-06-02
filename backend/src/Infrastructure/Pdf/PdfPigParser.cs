using System.Text.RegularExpressions;
using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SistemaTraction.Infrastructure.Pdf;

/// <summary>
/// Parser PDF com duas estratégias em cascata:
/// 1) Posicionamento por coluna — lê a coluna "SKU" pela posição das palavras.
///    É a estratégia correta para o UpSeller / Chrome-to-PDF, onde page.Text vem
///    como uma string contínua SEM espaços (inútil para regex), mas page.GetWords()
///    expõe cada palavra com coordenada X/Y. SKUs que quebram em duas linhas na
///    coluna estreita (ex.: "REG-REDR-RED-" + "GG") são remontados.
/// 2) Varredura de texto — fallback para PDFs cujo texto contém delimitadores.
/// </summary>
public class PdfPigParser : IPdfParser
{
    // SKU = ao menos 2 grupos maiúsculos separados por hífen.
    private static readonly Regex SkuRegex = new(
        @"(?<![A-Z0-9\-])[A-Z][A-Z0-9]{0,7}(?:-[A-Z0-9]{1,8}){2,4}(?=\s|$)",
        RegexOptions.Compiled | RegexOptions.Multiline);

    private static readonly Regex QtyRegex = new(@"^\s*(\d{1,4})\b", RegexOptions.Compiled);
    private static readonly Regex DigitsOnly = new(@"^\d{1,4}$", RegexOptions.Compiled);

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);

            // Estratégia 1: posicionamento por coluna (UpSeller e afins)
            var items = ParseByColumns(doc);
            if (items.Count > 0)
                return new ParseResult(true, items, null);

            // Estratégia 2: varredura de texto (PDFs com texto delimitado)
            var fallback = ParseByFullTextScan(doc);
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

    // ── Estratégia 1: posicionamento por coluna ───────────────────────────────────
    // 1. Localiza o cabeçalho "SKU" → define a coluna (X) dos SKUs.
    // 2. Junta as palavras dessa coluna abaixo do cabeçalho; remonta SKUs quebrados
    //    em duas linhas (fragmento sem hífen é continuação do SKU anterior).
    // 3. A quantidade é o número na coluna à direita, dentro da faixa vertical da linha.
    // 4. Acumula a quantidade por SKU (mesmo SKU repetido em vários pedidos soma).

    private static List<ParsedItem> ParseByColumns(PdfDocument doc)
    {
        const double xTol = 12.0;   // tolerância horizontal da coluna SKU
        const double yTol = 5.0;    // folga vertical ao casar a linha

        var rows = new List<(string Sku, int Qty)>();

        foreach (var page in doc.GetPages())
        {
            var words = page.GetWords()
                .Select(w => (
                    Y: page.Height - w.BoundingBox.Bottom,
                    X: w.BoundingBox.Left,
                    Text: NormalizeHyphens(w.Text).Trim()))
                .Where(w => w.Text.Length > 0)
                .ToList();

            var header = words.FirstOrDefault(w =>
                w.Text.Equals("SKU", StringComparison.OrdinalIgnoreCase));
            if (header.Text is null) continue;

            var skuX = header.X;
            var headerY = header.Y;

            // Palavras da coluna SKU, de cima para baixo
            var skuColumn = words
                .Where(w => Math.Abs(w.X - skuX) <= xTol && w.Y > headerY + 1)
                .OrderBy(w => w.Y)
                .ToList();
            if (skuColumn.Count == 0) continue;

            // Remonta SKUs quebrados: palavra com hífen inicia um SKU; sem hífen continua o anterior
            var skus = new List<(string Sku, double Y)>();
            foreach (var w in skuColumn)
            {
                if (w.Text.Contains('-') || skus.Count == 0)
                    skus.Add((w.Text, w.Y));
                else
                {
                    var last = skus[^1];
                    skus[^1] = (last.Sku + w.Text, last.Y);
                }
            }

            // Coluna de quantidade: números puros à direita da coluna SKU
            var qtyColumn = words
                .Where(w => w.X > skuX + 40 && DigitsOnly.IsMatch(w.Text))
                .OrderBy(w => w.Y)
                .ToList();

            // Altura típica de linha, para delimitar a faixa do último SKU
            var rowHeight = 50.0;
            if (skus.Count > 1)
            {
                var gaps = skus.Zip(skus.Skip(1), (a, b) => b.Y - a.Y).OrderBy(g => g).ToList();
                rowHeight = gaps[gaps.Count / 2];
            }

            // Casa cada SKU com a quantidade na sua faixa vertical
            for (var i = 0; i < skus.Count; i++)
            {
                var top = skus[i].Y - yTol;
                var bottom = (i + 1 < skus.Count ? skus[i + 1].Y : skus[i].Y + rowHeight) - yTol;
                var qtyWord = qtyColumn.FirstOrDefault(q => q.Y >= top && q.Y < bottom);
                var qty = qtyWord.Text is not null && int.TryParse(qtyWord.Text, out var q) && q is > 0 and < 9_999
                    ? q : 1;
                rows.Add((skus[i].Sku, qty));
            }
        }

        return Accumulate(rows);
    }

    // ── Estratégia 2: varredura de texto completo ─────────────────────────────────
    // Para PDFs cujo page.Text contém espaços/quebras entre os tokens.

    private static List<ParsedItem> ParseByFullTextScan(PdfDocument doc)
    {
        var rows = new List<(string Sku, int Qty)>();

        foreach (var page in doc.GetPages())
        {
            var text = NormalizeHyphens(page.Text ?? "");
            foreach (Match m in SkuRegex.Matches(text))
                rows.Add((m.Value.Trim(), ExtractQtyAfter(text, m.Index + m.Length)));
        }

        return Accumulate(rows);
    }

    private static int ExtractQtyAfter(string text, int start)
    {
        if (start >= text.Length) return 1;
        var segment = text[start..Math.Min(start + 20, text.Length)];
        var m = QtyRegex.Match(segment);
        return m.Success && int.TryParse(m.Groups[1].Value, out var q) && q is > 0 and < 9_999 ? q : 1;
    }

    // ── Utilitários ───────────────────────────────────────────────────────────────

    // Agrupa por SKU somando a quantidade, preservando a ordem de primeira aparição.
    private static List<ParsedItem> Accumulate(List<(string Sku, int Qty)> rows)
    {
        var order = new List<string>();
        var totals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var (sku, qty) in rows)
        {
            if (!totals.ContainsKey(sku)) order.Add(sku);
            totals[sku] = totals.GetValueOrDefault(sku) + qty;
        }

        return order
            .Select((sku, idx) => new ParsedItem(sku, "", "", totals[sku], idx))
            .ToList();
    }

    // Substitui variantes Unicode de hífen pelo hífen ASCII padrão
    private static string NormalizeHyphens(string text) =>
        Regex.Replace(text, @"[­‑‒–—−﹣－]", "-");
}
