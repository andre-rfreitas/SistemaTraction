using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Sewing;

public class SewerProductType : BaseEntity
{
    public Guid SewerId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public decimal PriceDefault { get; private set; }
    public decimal PriceG1 { get; private set; }

    public Sewer? Sewer { get; private set; }

    private SewerProductType() { }

    internal static SewerProductType Create(Guid sewerId, string name, decimal priceDefault, decimal priceG1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de produto não pode ser vazio.");

        if (priceDefault <= 0 || priceG1 <= 0)
            throw new DomainException("Preços do tipo de produto devem ser maiores que zero.");

        return new SewerProductType
        {
            SewerId = sewerId,
            Name = name.Trim(),
            PriceDefault = priceDefault,
            PriceG1 = priceG1
        };
    }

    internal void Update(string name, decimal priceDefault, decimal priceG1)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de produto não pode ser vazio.");

        if (priceDefault <= 0 || priceG1 <= 0)
            throw new DomainException("Preços do tipo de produto devem ser maiores que zero.");

        Name = name.Trim();
        PriceDefault = priceDefault;
        PriceG1 = priceG1;
        TouchUpdatedAt();
    }
}
