namespace SistemaTraction.Application.Common.Interfaces;

public interface IPdfParser
{
    ParseResult Parse(Stream pdfStream, string fileName);
}

public record ParseResult(bool Success, List<ParsedItem> Items, string? ErrorMessage);

public record ParsedItem(string Sku, string Color, string Size, int Quantity, int SortOrder);
