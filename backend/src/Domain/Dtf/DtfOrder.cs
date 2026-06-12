using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfOrder : BaseEntity
{
    public int OrderNumber { get; private set; }
    public DtfOrderStatus Status { get; private set; }
    public string? Notes { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReceivedAt { get; private set; }

    private readonly List<DtfOrderItem> _items = [];
    // Filters soft-deleted items — RemoveItem() uses MarkAsDeleted() for EF tracking.
    public IReadOnlyList<DtfOrderItem> Items => _items.Where(i => !i.IsDeleted).ToList().AsReadOnly();

    private DtfOrder() { }

    public static DtfOrder Create(int orderNumber, List<(Guid DtfModelId, int SheetQuantity)> items, string? notes)
    {
        if (items.Count == 0)
            throw new DomainException("O pedido deve ter pelo menos um modelo.");

        var distinctModels = items.Select(i => i.DtfModelId).Distinct().Count();
        if (distinctModels != items.Count)
            throw new DomainException("Não é permitido adicionar o mesmo modelo duplicado no pedido.");

        var order = new DtfOrder
        {
            OrderNumber = orderNumber,
            Status = DtfOrderStatus.Draft,
            Notes = notes?.Trim(),
        };

        foreach (var (modelId, sheetQty) in items)
            order._items.Add(DtfOrderItem.Create(order.Id, modelId, sheetQty));

        return order;
    }

    public void AddItem(Guid dtfModelId, int sheetQuantity)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Itens só podem ser alterados em pedidos em rascunho.");

        if (_items.Any(i => i.DtfModelId == dtfModelId && !i.IsDeleted))
            throw new DomainException("Não é permitido adicionar o mesmo modelo duplicado no pedido.");

        _items.Add(DtfOrderItem.Create(Id, dtfModelId, sheetQuantity));
        TouchUpdatedAt();
    }

    public void RemoveItem(Guid dtfModelId)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Itens só podem ser alterados em pedidos em rascunho.");

        var item = _items.FirstOrDefault(i => i.DtfModelId == dtfModelId && !i.IsDeleted)
            ?? throw new DomainException("Modelo não encontrado no pedido.");

        item.MarkAsDeleted();
        TouchUpdatedAt();
    }

    public void UpdateNotes(string? notes)
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Notas só podem ser alteradas em pedidos em rascunho.");
        Notes = notes?.Trim();
        TouchUpdatedAt();
    }

    public void MarkSent()
    {
        if (Status != DtfOrderStatus.Draft)
            throw new DomainException("Apenas pedidos em rascunho podem ser enviados.");

        Status = DtfOrderStatus.Sent;
        SentAt = DateTime.UtcNow;
        TouchUpdatedAt();
    }

    public void MarkReceived()
    {
        if (Status != DtfOrderStatus.Sent)
            throw new DomainException("Apenas pedidos enviados podem ser marcados como recebidos.");

        Status = DtfOrderStatus.Received;
        ReceivedAt = DateTime.UtcNow;
        TouchUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == DtfOrderStatus.Received)
            throw new DomainException("Pedidos já recebidos não podem ser cancelados.");
        MarkAsDeleted();
    }
}
