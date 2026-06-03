using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfStockMovement : BaseEntity
{
    public Guid DtfStockItemId { get; private set; }
    public DtfStockItem DtfStockItem { get; private set; } = null!;
    public DtfMovementType Type { get; private set; }

    /// <summary>Delta aplicado ao estoque em estampas: positivo = entrada, negativo = saída.</summary>
    public int Delta { get; private set; }

    public string? Reason { get; private set; }

    /// <summary>Número de folhas recebidas na Entrada. Null em Saída/Ajuste.</summary>
    public int? SheetCount { get; private set; }

    private DtfStockMovement() { }

    internal static DtfStockMovement Create(
        Guid stockItemId, DtfMovementType type, int delta, string? reason, int? sheetCount = null) =>
        new()
        {
            DtfStockItemId = stockItemId,
            Type = type,
            Delta = delta,
            Reason = reason?.Trim(),
            SheetCount = sheetCount
        };
}
