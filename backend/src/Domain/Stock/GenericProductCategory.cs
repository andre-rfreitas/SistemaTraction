using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class GenericProductCategory : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    private GenericProductCategory() { }

    public static GenericProductCategory Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da categoria é obrigatório.");

        return new GenericProductCategory { Name = name.Trim() };
    }
}
