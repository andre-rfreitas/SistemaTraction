using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyStockMovement : BaseEntity
{
    public Guid SupplyStockItemId { get; private set; }
    public SupplyStockItem SupplyStockItem { get; private set; } = null!;
    public SupplyMovementType Type { get; private set; }
    public int Delta { get; private set; }
    public string? Reason { get; private set; }

    private SupplyStockMovement() { }

    internal static SupplyStockMovement Create(
        Guid supplyStockItemId, SupplyMovementType type, int delta, string? reason) =>
        new()
        {
            SupplyStockItemId = supplyStockItemId,
            Type = type,
            Delta = delta,
            Reason = reason
        };
}
