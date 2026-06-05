namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyStockMovementDto(
    Guid Id,
    string Type,
    int Delta,
    string? Reason,
    DateTime CreatedAt);
