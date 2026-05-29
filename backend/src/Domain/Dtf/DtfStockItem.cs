using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfStockItem : BaseEntity
{
    public Guid DtfModelId { get; private set; }
    public DtfModel DtfModel { get; private set; } = null!;
    public int CurrentQuantity { get; private set; }

    private readonly List<DtfStockMovement> _movements = [];
    public IReadOnlyCollection<DtfStockMovement> Movements => _movements.AsReadOnly();

    private DtfStockItem() { }

    public static DtfStockItem Create(Guid dtfModelId) =>
        new() { DtfModelId = dtfModelId };

    /// <summary>
    /// Registra uma movimentação de estoque (append-only).
    /// Para Entrada/Saida: quantity deve ser positivo.
    /// Para Ajuste: quantity é o delta assinado (positivo ou negativo).
    /// </summary>
    public DtfStockMovement AddMovement(DtfMovementType type, int quantity, string? reason = null)
    {
        if (type != DtfMovementType.Ajuste && quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        if (type == DtfMovementType.Ajuste && quantity == 0)
            throw new DomainException("Delta de ajuste não pode ser zero.");

        var delta = type switch
        {
            DtfMovementType.Entrada => quantity,
            DtfMovementType.Saida   => -quantity,
            DtfMovementType.Ajuste  => quantity,
            _ => throw new DomainException("Tipo de movimentação inválido.")
        };

        if (CurrentQuantity + delta < 0)
            throw new DomainException(
                $"Estoque insuficiente. Atual: {CurrentQuantity}, solicitado: {quantity}.");

        CurrentQuantity += delta;
        TouchUpdatedAt();

        var movement = DtfStockMovement.Create(Id, type, delta, reason);
        _movements.Add(movement);
        return movement;
    }
}
