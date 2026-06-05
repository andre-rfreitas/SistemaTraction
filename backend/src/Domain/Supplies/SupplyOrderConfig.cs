using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyOrderConfig : BaseEntity
{
    public Guid SupplyTypeId { get; private set; }
    public SupplyType SupplyType { get; private set; } = null!;
    public int QuantityPerOrder { get; private set; }

    private SupplyOrderConfig() { }

    public static SupplyOrderConfig Create(Guid supplyTypeId, int quantityPerOrder)
    {
        if (quantityPerOrder < 0)
            throw new DomainException("Quantidade por pedido não pode ser negativa.");

        return new SupplyOrderConfig { SupplyTypeId = supplyTypeId, QuantityPerOrder = quantityPerOrder };
    }

    public void Update(int quantityPerOrder)
    {
        if (quantityPerOrder < 0)
            throw new DomainException("Quantidade por pedido não pode ser negativa.");

        QuantityPerOrder = quantityPerOrder;
        TouchUpdatedAt();
    }
}
