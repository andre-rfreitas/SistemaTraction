namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyStockItemDto(
    Guid Id,
    Guid SupplyTypeId,
    string Name,
    string Unit,
    int Quantity);
