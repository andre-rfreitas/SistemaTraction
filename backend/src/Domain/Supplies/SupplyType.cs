using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal? PricePerUnit { get; private set; }
    public YieldBasis YieldBasis { get; private set; } = YieldBasis.None;
    public decimal? YieldQuantity { get; private set; }
    public string? YieldProductName { get; private set; }

    private SupplyType() { }

    public static SupplyType Create(string name, string unit, decimal? pricePerUnit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de insumo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Unidade do tipo de insumo não pode ser vazia.");

        if (pricePerUnit.HasValue && pricePerUnit.Value < 0)
            throw new DomainException("Preço por unidade não pode ser negativo.");

        return new SupplyType { Name = name.Trim(), Unit = unit.Trim(), PricePerUnit = pricePerUnit };
    }

    public void Update(string name, string unit, decimal? pricePerUnit = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de insumo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Unidade do tipo de insumo não pode ser vazia.");

        if (pricePerUnit.HasValue && pricePerUnit.Value < 0)
            throw new DomainException("Preço por unidade não pode ser negativo.");

        Name = name.Trim();
        Unit = unit.Trim();
        PricePerUnit = pricePerUnit;
        TouchUpdatedAt();
    }

    public void SetYield(YieldBasis basis, decimal quantity, string? productName)
    {
        if (basis == YieldBasis.None)
        {
            ClearYield();
            return;
        }

        if (quantity <= 0)
            throw new DomainException("Quantidade do rendimento deve ser maior que zero.");

        if (basis == YieldBasis.PerProduct && string.IsNullOrWhiteSpace(productName))
            throw new DomainException("Nome do produto é obrigatório quando o rendimento é por produto.");

        YieldBasis = basis;
        YieldQuantity = quantity;
        YieldProductName = basis == YieldBasis.PerProduct ? productName!.Trim() : null;
        TouchUpdatedAt();
    }

    public void ClearYield()
    {
        YieldBasis = YieldBasis.None;
        YieldQuantity = null;
        YieldProductName = null;
        TouchUpdatedAt();
    }
}
