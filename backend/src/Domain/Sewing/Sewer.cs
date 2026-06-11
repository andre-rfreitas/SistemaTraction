using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Sewing;

public class Sewer : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public bool IsActive { get; private set; } = true;

    private readonly List<SewerProductType> _productTypes = [];
    public IReadOnlyList<SewerProductType> ProductTypes => _productTypes.AsReadOnly();

    private Sewer() { }

    public static Sewer Create(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da costureira não pode ser vazio.");

        return new Sewer
        {
            Name = name.Trim(),
            Phone = phone?.Trim(),
        };
    }

    public void Update(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da costureira não pode ser vazio.");

        Name = name.Trim();
        Phone = phone?.Trim();
        TouchUpdatedAt();
    }

    public void Deactivate() { IsActive = false; TouchUpdatedAt(); }
    public void Activate() { IsActive = true; TouchUpdatedAt(); }

    public SewerProductType AddProductType(string name, decimal priceDefault, decimal priceG1)
    {
        var pt = SewerProductType.Create(Id, name, priceDefault, priceG1);
        _productTypes.Add(pt);
        return pt;
    }

    public void UpdateProductType(Guid productTypeId, string name, decimal priceDefault, decimal priceG1)
    {
        var pt = _productTypes.FirstOrDefault(p => p.Id == productTypeId && !p.IsDeleted)
            ?? throw new DomainException("Tipo de produto não encontrado.");
        pt.Update(name, priceDefault, priceG1);
    }

    public void RemoveProductType(Guid productTypeId)
    {
        var pt = _productTypes.FirstOrDefault(p => p.Id == productTypeId && !p.IsDeleted)
            ?? throw new DomainException("Tipo de produto não encontrado.");
        pt.MarkAsDeleted();
    }
}
