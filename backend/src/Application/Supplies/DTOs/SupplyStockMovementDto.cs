namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyStockMovementDto(
    Guid Id,
    string Type,
    int Delta,
    string? Reason,
    string? SupplierName,
    string? SupplierPhone,
    DateTime OccurredAt,
    decimal? UnitPrice,
    decimal? TotalCost,
    DateTime CreatedAt);
