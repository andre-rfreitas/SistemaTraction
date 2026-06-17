using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class ShirtStockMovement : BaseEntity
{
    public Guid StockItemId { get; private set; }
    public Guid FabricColorId { get; private set; }
    public string FabricColorName { get; private set; } = "";
    public string Size { get; private set; } = "";
    public ShirtType ShirtType { get; private set; } = ShirtType.Regular;

    /// <summary>Positivo = entrada, negativo = saída.</summary>
    public int Delta { get; private set; }

    public string Reason { get; private set; } = "";

    /// <summary>Manual | Costureiro | Separação</summary>
    public string Origin { get; private set; } = "";

    public Guid? ReferenceId { get; private set; }

    public StockItem? StockItem { get; private set; }

    private ShirtStockMovement() { }

    public static ShirtStockMovement Create(
        Guid stockItemId,
        Guid fabricColorId,
        string fabricColorName,
        string size,
        int delta,
        string reason,
        string origin,
        Guid? referenceId = null,
        ShirtType shirtType = ShirtType.Regular)
    {
        if (delta == 0)
            throw new DomainException("Delta da movimentação não pode ser zero.");

        return new ShirtStockMovement
        {
            StockItemId = stockItemId,
            FabricColorId = fabricColorId,
            FabricColorName = fabricColorName.Trim(),
            Size = size.Trim().ToUpper(),
            ShirtType = shirtType,
            Delta = delta,
            Reason = reason.Trim(),
            Origin = origin.Trim(),
            ReferenceId = referenceId
        };
    }
}
