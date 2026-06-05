namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyDeductionPreviewItem(
    Guid SupplyTypeId,
    Guid SupplyStockItemId,
    string Name,
    string Unit,
    int QuantityPerOrder,
    int TotalQuantity);
