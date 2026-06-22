using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class ShirtStockMovement : BaseEntity
{
    public Guid StockItemId { get; private set; }
    public Guid FabricColorId { get; private set; }
    public string FabricColorName { get; private set; } = "";
    public string Size { get; private set; } = "";
    public string ModelCode { get; private set; } = "";

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
        string modelCode = "REG")
    {
        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");
        if (delta == 0)
            throw new DomainException("Delta não pode ser zero.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Motivo é obrigatório.");

        return new ShirtStockMovement
        {
            StockItemId = stockItemId,
            FabricColorId = fabricColorId,
            FabricColorName = fabricColorName.Trim(),
            Size = size.Trim().ToUpper(),
            ModelCode = string.IsNullOrWhiteSpace(modelCode) ? "REG" : modelCode.Trim().ToUpper(),
            Delta = delta,
            Reason = reason.Trim(),
            Origin = origin.Trim(),
            ReferenceId = referenceId
        };
    }
}
