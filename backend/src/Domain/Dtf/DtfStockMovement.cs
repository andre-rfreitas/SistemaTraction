using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfStockMovement : BaseEntity
{
    public Guid DtfStockItemId { get; private set; }
    public DtfStockItem DtfStockItem { get; private set; } = null!;
    public DtfMovementType Type { get; private set; }

    /// <summary>Delta aplicado ao estoque: positivo = entrada, negativo = saída.</summary>
    public int Delta { get; private set; }

    public string? Reason { get; private set; }

    private DtfStockMovement() { }

    internal static DtfStockMovement Create(
        Guid stockItemId, DtfMovementType type, int delta, string? reason) =>
        new()
        {
            DtfStockItemId = stockItemId,
            Type = type,
            Delta = delta,
            Reason = reason?.Trim()
        };
}
