using System.Text.RegularExpressions;
using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SistemaTraction.Infrastructure.Pdf;

public class PdfPigParser : IPdfParser
{
    // SKU pattern: two or more hyphen-separated uppercase alphanumeric groups
    private static readonly Regex SkuRegex =
        new(@"^[A-Z][A-Z0-9]+-[A-Z0-9]+(-[A-Z0-9]+)*$", RegexOptions.Compiled);

    private static readonly HashSet<string> KnownSizes =
        new(StringComparer.OrdinalIgnoreCase) { "P", "M", "G", "GG", "G1", "XG", "XGG", "PP", "GGG" };

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);
            var rows = ExtractRows(doc);

            // Try SKU-based strategy (UpSeller and similar)
            var skuItems = ParseSkuBased(rows);
            if (skuItems.Count > 0)
                return new ParseResult(true, skuItems, null);

            // Fall back to size-token strategy (generic ERPs)
            var legacyItems = ParseLegacy(rows);
            if (legacyItems.Count > 0)
                return new ParseResult(true, legacyItems, null);

            return new ParseResult(false, [],
                "Nenhum item encontrado no PDF. Verifique se o arquivo é uma lista de separação válida do ERP.");
        }
        catch (Exception ex)
        {
            return new ParseResult(false, [], $"Erro ao ler o PDF: {ex.Message}");
        }
    }

    // ── SKU-based strategy (UpSeller format) ─────────────────────────────────
    // Finds tokens matching the SKU regex and the quantity on the same row.
    // Groups identical SKUs and sums quantities.

    private static List<ParsedItem> ParseSkuBased(List<List<string>> rows)
    {
        var accumulated = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            string? sku = null;
            int qty = 0;

            for (int i = 0; i < row.Count; i++)
            {
                var token = row[i];
                if (sku == null && SkuRegex.IsMatch(token))
                {
                    sku = token;
                    // Look for quantity immediately after the SKU
                    if (i + 1 < row.Count && int.TryParse(row[i + 1], out var q) && q > 0 && q < 10_000)
                        qty = q;
                    break;
                }
            }

            // If qty not found after SKU, try the last numeric token on the row
            if (sku != null && qty == 0)
            {
                for (int i = row.Count - 1; i >= 0; i--)
                {
                    if (int.TryParse(row[i], out var q) && q > 0 && q < 10_000)
                    {
                        qty = q;
                        break;
                    }
                }
            }

            if (sku != null && qty > 0)
                accumulated[sku] = accumulated.GetValueOrDefault(sku) + qty;
        }

        return accumulated
            .Select((kv, idx) => new ParsedItem(kv.Key, "", "", kv.Value, idx))
            .ToList();
    }

    // ── Legacy size-token strategy ────────────────────────────────────────────
    // Identifies rows by a known size token and extracts color + quantity.

    private static List<ParsedItem> ParseLegacy(List<List<string>> rows)
    {
        var items = new List<ParsedItem>();
        var sortOrder = 0;

        foreach (var row in rows)
        {
            var sizeIndex = row.FindIndex(t => KnownSizes.Contains(t));
            if (sizeIndex < 0) continue;

            var qty = 0;
            for (int i = sizeIndex + 1; i < row.Count; i++)
            {
                if (int.TryParse(row[i], out var q) && q > 0) { qty = q; break; }
            }
            if (qty == 0) continue;

            var size = row[sizeIndex];
            var color = sizeIndex > 0 && !int.TryParse(row[sizeIndex - 1], out _) ? row[sizeIndex - 1] : "?";
            var sku = sizeIndex > 1 ? string.Join(" ", row.Take(sizeIndex - 1)) : "";

            if (color == "?" || IsHeaderWord(color)) continue;

            items.Add(new ParsedItem(sku.Trim(), color.Trim(), size, qty, sortOrder++));
        }

        return items;
    }

    // ── PDF word extraction ───────────────────────────────────────────────────

    private static List<List<string>> ExtractRows(PdfDocument doc)
    {
        const double rowTolerance = 4.0;
        var allWords = new List<(double Y, double X, string Text)>();

        foreach (var page in doc.GetPages())
        {
            var height = page.Height;
            foreach (var word in page.GetWords())
                allWords.Add((height - word.BoundingBox.Bottom, word.BoundingBox.Left, word.Text));
        }

        return allWords
            .GroupBy(w => Math.Round(w.Y / rowTolerance) * rowTolerance)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(w => w.X).Select(w => w.Text).ToList())
            .Where(r => r.Count > 0)
            .ToList();
    }

    private static bool IsHeaderWord(string text) =>
        text.Equals("Cor", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Tamanho", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Qtd", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Quantidade", StringComparison.OrdinalIgnoreCase);
}
