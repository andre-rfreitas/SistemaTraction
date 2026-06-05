using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;

    private SupplyType() { }

    public static SupplyType Create(string name, string unit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de insumo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Unidade do tipo de insumo não pode ser vazia.");

        return new SupplyType { Name = name.Trim(), Unit = unit.Trim() };
    }

    public void Update(string name, string unit)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de insumo não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(unit))
            throw new DomainException("Unidade do tipo de insumo não pode ser vazia.");

        Name = name.Trim();
        Unit = unit.Trim();
        TouchUpdatedAt();
    }
}
