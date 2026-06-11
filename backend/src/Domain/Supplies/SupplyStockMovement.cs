using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyStockMovement : BaseEntity
{
    public Guid SupplyStockItemId { get; private set; }
    public SupplyStockItem SupplyStockItem { get; private set; } = null!;
    public SupplyMovementType Type { get; private set; }
    public int Delta { get; private set; }
    public string? Reason { get; private set; }
    public string? SupplierName { get; private set; }
    public string? SupplierPhone { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public decimal? TotalCost { get; private set; }

    private SupplyStockMovement() { }

    internal static SupplyStockMovement Create(
        Guid supplyStockItemId,
        SupplyMovementType type,
        int delta,
        string? reason,
        string? supplierName = null,
        string? supplierPhone = null,
        DateTime? occurredAt = null,
        decimal? unitPrice = null,
        decimal? totalCost = null) =>
        new()
        {
            SupplyStockItemId = supplyStockItemId,
            Type = type,
            Delta = delta,
            Reason = reason,
            SupplierName = supplierName?.Trim(),
            SupplierPhone = supplierPhone?.Trim(),
            OccurredAt = occurredAt ?? DateTime.UtcNow,
            UnitPrice = unitPrice,
            TotalCost = totalCost,
        };

    public void UpdateMetadata(
        string? supplierName,
        string? supplierPhone,
        DateTime? occurredAt,
        decimal? unitPrice,
        decimal? totalCost)
    {
        SupplierName = supplierName?.Trim();
        SupplierPhone = supplierPhone?.Trim();
        if (occurredAt.HasValue)
            OccurredAt = occurredAt.Value;
        UnitPrice = unitPrice;
        TotalCost = totalCost;
        TouchUpdatedAt();
    }
}
