namespace SistemaTraction.Application.Financial.DTOs;

public record FinancialEntryDto(
    Guid Id,
    string Type,
    string Category,
    decimal Amount,
    string Description,
    Guid? ReferenceId,
    string? ReferenceType,
    DateTime EntryDate,
    DateTime CreatedAt
);
