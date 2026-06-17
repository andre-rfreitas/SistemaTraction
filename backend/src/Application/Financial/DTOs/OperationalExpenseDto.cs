namespace SistemaTraction.Application.Financial.DTOs;

public record OperationalExpenseDto(
    Guid Id,
    string Name,
    decimal FixedMonthlyValue,
    decimal RatePercent,
    bool IsActive
);

public record OpexPeriodItemDto(
    Guid Id,
    string Name,
    decimal FixedMonthlyValue,
    decimal RatePercent,
    decimal ProratedFixed,
    decimal RateAmount,
    decimal PeriodTotal
);
