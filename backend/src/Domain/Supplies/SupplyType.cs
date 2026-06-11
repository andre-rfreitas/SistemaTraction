using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public decimal? PricePerUnit { get; private set; }

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
}
