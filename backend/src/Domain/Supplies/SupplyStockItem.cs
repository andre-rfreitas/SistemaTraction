using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Supplies;

public class SupplyStockItem : BaseEntity
{
    public Guid SupplyTypeId { get; private set; }
    public SupplyType SupplyType { get; private set; } = null!;
    public int Quantity { get; private set; }

    private readonly List<SupplyStockMovement> _movements = [];
    public IReadOnlyCollection<SupplyStockMovement> Movements => _movements.AsReadOnly();

    private SupplyStockItem() { }

    public static SupplyStockItem Create(Guid supplyTypeId) =>
        new() { SupplyTypeId = supplyTypeId };

    public SupplyStockMovement AddMovement(
        SupplyMovementType type,
        int quantity,
        string? reason = null,
        string? supplierName = null,
        string? supplierPhone = null,
        DateTime? occurredAt = null,
        decimal? unitPrice = null,
        decimal? totalCost = null)
    {
        if (type != SupplyMovementType.Ajuste && quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        if (type == SupplyMovementType.Ajuste && quantity == 0)
            throw new DomainException("Delta de ajuste não pode ser zero.");

        var delta = type switch
        {
            SupplyMovementType.Entrada => quantity,
            SupplyMovementType.Saida   => -quantity,
            SupplyMovementType.Ajuste  => quantity,
            _ => throw new DomainException("Tipo de movimentação inválido.")
        };

        if (Quantity + delta < 0)
            throw new DomainException(
                $"Estoque insuficiente. Atual: {Quantity}, solicitado: {quantity}.");

        Quantity += delta;
        TouchUpdatedAt();

        var movement = SupplyStockMovement.Create(Id, type, delta, reason, supplierName, supplierPhone, occurredAt, unitPrice, totalCost);
        _movements.Add(movement);
        return movement;
    }
}
