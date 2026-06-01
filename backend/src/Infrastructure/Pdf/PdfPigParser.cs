using SistemaTraction.Application.Common.Interfaces;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SistemaTraction.Infrastructure.Pdf;

public class PdfPigParser : IPdfParser
{
    private static readonly HashSet<string> KnownSizes =
        new(StringComparer.OrdinalIgnoreCase) { "P", "M", "G", "GG", "G1", "XG", "XGG", "PP", "GGG" };

    public ParseResult Parse(Stream pdfStream, string fileName)
    {
        try
        {
            using var doc = PdfDocument.Open(pdfStream);
            var rows = ExtractRows(doc);
            var items = ParseItems(rows);

            if (items.Count == 0)
                return new ParseResult(false, [], "Nenhum item com SKU/Cor/Tamanho/Quantidade encontrado. " +
                    "Verifique se o PDF é uma lista de separação do ERP com colunas de Cor, Tamanho e Quantidade.");

            return new ParseResult(true, items, null);
        }
        catch (Exception ex)
        {
            return new ParseResult(false, [], $"Erro ao ler o PDF: {ex.Message}");
        }
    }

    private static List<List<Word>> ExtractRows(PdfDocument doc)
    {
        const double rowTolerance = 4.0;
        var allWords = new List<(double X, double Y, string Text)>();

        foreach (var page in doc.GetPages())
        {
            var height = page.Height;
            foreach (var word in page.GetWords())
            {
                // Normalise Y so rows with similar Y are grouped together
                allWords.Add((word.BoundingBox.Left, height - word.BoundingBox.Bottom, word.Text));
            }
        }

        // Group words by Y position (same row = within rowTolerance px of each other)
        var grouped = allWords
            .GroupBy(w => Math.Round(w.Y / rowTolerance) * rowTolerance)
            .OrderBy(g => g.Key)
            .Select(g => g.OrderBy(w => w.X).Select(w => new Word(w.Text, w.X)).ToList())
            .ToList();

        return grouped;
    }

    private static List<ParsedItem> ParseItems(List<List<Word>> rows)
    {
        var items = new List<ParsedItem>();
        var sortOrder = 0;

        foreach (var row in rows)
        {
            var texts = row.Select(w => w.Text).ToList();
            var item = TryParseRow(texts, sortOrder);
            if (item is not null)
            {
                items.Add(item);
                sortOrder++;
            }
        }

        return items;
    }

    private static ParsedItem? TryParseRow(List<string> tokens, int sortOrder)
    {
        // Find a size token — that anchors the row
        var sizeIndex = tokens.FindIndex(t => KnownSizes.Contains(t));
        if (sizeIndex < 0) return null;

        // Quantity must be an integer immediately after the size, or among the last few tokens
        var qty = 0;
        var qtyIndex = -1;

        for (var i = sizeIndex + 1; i < tokens.Count; i++)
        {
            if (int.TryParse(tokens[i], out var q) && q > 0)
            {
                qty = q;
                qtyIndex = i;
                break;
            }
        }

        if (qtyIndex < 0) return null;

        var size = tokens[sizeIndex];

        // Color = token immediately before size (if it's not a number)
        var color = sizeIndex > 0 && !int.TryParse(tokens[sizeIndex - 1], out _)
            ? tokens[sizeIndex - 1]
            : "?";

        // SKU = everything before color, joined
        var sku = sizeIndex > 1
            ? string.Join(" ", tokens.Take(sizeIndex - 1))
            : "";

        // Skip header-like rows
        if (color == "?" || IsHeader(color)) return null;

        return new ParsedItem(sku.Trim(), color.Trim(), size.ToUpper(), qty, sortOrder);
    }

    private static bool IsHeader(string text) =>
        text.Equals("Cor", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Color", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Tamanho", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Size", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Qtd", StringComparison.OrdinalIgnoreCase) ||
        text.Equals("Quantidade", StringComparison.OrdinalIgnoreCase);

    private record Word(string Text, double X);
}
