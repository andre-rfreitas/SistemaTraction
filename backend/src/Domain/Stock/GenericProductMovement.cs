using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Stock;

public class GenericProductMovement : BaseEntity
{
    public Guid GenericProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public int Delta { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public string Origin { get; private set; } = string.Empty;

    public GenericProduct? GenericProduct { get; private set; }

    private GenericProductMovement() { }

    public static GenericProductMovement Create(
        Guid genericProductId,
        string productName,
        int delta,
        string reason,
        string origin)
    {
        if (delta == 0)
            throw new DomainException("Delta da movimentação não pode ser zero.");

        return new GenericProductMovement
        {
            GenericProductId = genericProductId,
            ProductName = productName.Trim(),
            Delta = delta,
            Reason = reason.Trim(),
            Origin = origin.Trim()
        };
    }
}
