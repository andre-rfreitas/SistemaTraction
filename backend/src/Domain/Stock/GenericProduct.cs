using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class GenericProduct : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    public GenericProductCategory? Category { get; private set; }

    private GenericProduct() { }

    public static GenericProduct Create(Guid categoryId, string name, int initialQuantity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do produto é obrigatório.");
            
        if (initialQuantity < 0)
            throw new DomainException("Quantidade inicial não pode ser negativa.");

        return new GenericProduct
        {
            CategoryId = categoryId,
            Name = name.Trim(),
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

    public void UseFromStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        if (quantity > Quantity)
            throw new DomainException($"Estoque insuficiente para '{Name}'. Disponível: {Quantity}, solicitado: {quantity}.");

        Quantity -= quantity;
        TouchUpdatedAt();
    }
}
