using System.Text.Json;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Cutting;

public class CuttingOrder : BaseEntity
{
    public int OrderNumber { get; private set; }
    public CuttingOrderStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Notes { get; private set; }

    public string? RecommendedPiecesJson { get; private set; }
    public int? RecommendationDays { get; private set; }
    public int? RecommendationBasedOnOrders { get; private set; }

    private readonly List<CuttingOrderItem> _items = [];
    public IReadOnlyList<CuttingOrderItem> Items => _items.AsReadOnly();

    private CuttingOrder() { }

    public static CuttingOrder Create(
        int orderNumber,
        List<(Guid FabricRollId, Dictionary<string, int> Pieces)> items,
        string? notes = null)
    {
        if (items.Count == 0)
            throw new DomainException("O pedido deve ter pelo menos uma bobina.");

        foreach (var (_, pieces) in items)
        {
            if (pieces.Values.Any(v => v < 0))
                throw new DomainException("Quantidades não podem ser negativas.");
            if (pieces.Values.Sum() == 0)
                throw new DomainException("Cada bobina deve ter pelo menos uma peça.");
        }

        var order = new CuttingOrder
        {
            OrderNumber = orderNumber,
            Status = CuttingOrderStatus.Draft,
            Notes = notes?.Trim()
        };

        foreach (var (rollId, pieces) in items)
            order._items.Add(CuttingOrderItem.Create(order.Id, rollId, pieces));

        return order;
    }

    public void SetRecommendationSnapshot(Dictionary<string, int>? recommendedPieces, int? days, int? basedOnOrders)
    {
        RecommendedPiecesJson = recommendedPieces is not null
            ? JsonSerializer.Serialize(recommendedPieces)
            : null;
        RecommendationDays = days;
        RecommendationBasedOnOrders = basedOnOrders;
        TouchUpdatedAt();
    }

    public Dictionary<string, int> GetRequestedPieces()
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in _items)
            foreach (var (size, qty) in item.GetRequestedPieces())
                result[size] = result.GetValueOrDefault(size) + qty;
        return result;
    }

    public Dictionary<string, int>? GetRecommendedPieces()
        => RecommendedPiecesJson is not null
            ? JsonSerializer.Deserialize<Dictionary<string, int>>(RecommendedPiecesJson)
            : null;

    public int GetTotalPieces() => _items.Sum(i => i.GetTotalPieces());

    public void UpdateItems(List<(Guid FabricRollId, Dictionary<string, int> Pieces)> items)
    {
        if (Status != CuttingOrderStatus.Draft)
            throw new DomainException("Apenas pedidos em rascunho podem ser editados.");

        foreach (var (rollId, pieces) in items)
        {
            var item = _items.FirstOrDefault(i => i.FabricRollId == rollId)
                ?? throw new DomainException($"Bobina não encontrada no pedido.");
            item.UpdatePieces(pieces);
        }
        TouchUpdatedAt();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        TouchUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == CuttingOrderStatus.Delivered || Status == CuttingOrderStatus.SewingDelivered)
            throw new DomainException("Pedidos já entregues não podem ser cancelados manualmente. Use o estorno financeiro.");
        Status = CuttingOrderStatus.Cancelled;
        TouchUpdatedAt();
    }

    public void CancelDelivered()
    {
        Status = CuttingOrderStatus.Cancelled;
        TouchUpdatedAt();
    }

    public void MarkSent()
    {
        if (Status != CuttingOrderStatus.Draft)
            throw new DomainException("Apenas pedidos em rascunho podem ser enviados.");

        Status = CuttingOrderStatus.SentToCutter;
        SentAt = DateTime.UtcNow;
        TouchUpdatedAt();
    }

    public void MarkDelivered()
    {
        if (Status != CuttingOrderStatus.SentToCutter)
            throw new DomainException("Apenas pedidos enviados podem ser marcados como entregues.");

        Status = CuttingOrderStatus.Delivered;
        TouchUpdatedAt();
    }

    public void MarkSewingDelivered()
    {
        if (Status != CuttingOrderStatus.Delivered)
            throw new DomainException("Apenas pedidos entregues pelo cortador podem ter entrega do costureiro registrada.");

        Status = CuttingOrderStatus.SewingDelivered;
        TouchUpdatedAt();
    }
}
