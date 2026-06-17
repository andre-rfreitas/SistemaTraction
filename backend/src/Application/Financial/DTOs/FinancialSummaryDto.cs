namespace SistemaTraction.Application.Financial.DTOs;

public record CategoryTotalDto(
    string Category,
    decimal Total
);

public record FinancialSummaryDto(
    DateTime From,
    DateTime To,
    decimal TotalFabric,
    decimal TotalCutting,
    decimal TotalSewing,
    decimal TotalDefects,
    decimal TotalDtf,
    decimal TotalIncome,
    decimal TotalExpense,
    decimal Balance,
    int GoodPiecesProduced,
    decimal? AverageCostPerShirt,
    List<CategoryTotalDto> ExpenseByCategory,
    decimal TotalOpex,
    List<OpexPeriodItemDto> OpexItems
);
