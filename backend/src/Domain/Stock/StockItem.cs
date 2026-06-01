using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class StockItem : BaseEntity
{
    public Guid FabricColorId { get; private set; }
    public string FabricColorName { get; private set; } = "";
    public string FabricTypeName { get; private set; } = "";
    public string FabricTypeVariation { get; private set; } = "";
    public string Size { get; private set; } = "";
    public int Quantity { get; private set; }

    private StockItem() { }

    public static StockItem Create(
        Guid fabricColorId,
        string fabricColorName,
        string fabricTypeName,
        string fabricTypeVariation,
        string size,
        int initialQuantity)
    {
        if (initialQuantity < 0)
            throw new DomainException("Quantidade inicial não pode ser negativa.");

        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");

        return new StockItem
        {
            FabricColorId = fabricColorId,
            FabricColorName = fabricColorName.Trim(),
            FabricTypeName = fabricTypeName.Trim(),
            FabricTypeVariation = fabricTypeVariation.Trim(),
            Size = size.Trim().ToUpper(),
            Quantity = initialQuantity
        };
    }

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade a adicionar deve ser maior que zero.");

        Quantity += quantity;
        TouchUpdatedAt();
    }
}
