using System.Text.Json;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Domain.Cutting;

public class CuttingOrder : BaseEntity
{
    public int OrderNumber { get; private set; }
    public Guid FabricRollId { get; private set; }
    public string RequestedPiecesJson { get; private set; } = "{}";
    public CuttingOrderStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public string? Notes { get; private set; }

    public FabricRoll? FabricRoll { get; private set; }

    private CuttingOrder() { }

    public static CuttingOrder Create(int orderNumber, Guid fabricRollId, Dictionary<string, int> requestedPieces, string? notes = null)
    {
        if (requestedPieces.Values.Any(v => v < 0))
            throw new DomainException("Quantidades não podem ser negativas.");

        if (requestedPieces.Values.Sum() == 0)
            throw new DomainException("O pedido deve ter pelo menos uma peça.");

        return new CuttingOrder
        {
            OrderNumber = orderNumber,
            FabricRollId = fabricRollId,
            RequestedPiecesJson = JsonSerializer.Serialize(requestedPieces),
            Status = CuttingOrderStatus.Draft,
            Notes = notes?.Trim()
        };
    }

    public Dictionary<string, int> GetRequestedPieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(RequestedPiecesJson) ?? [];

    public int GetTotalPieces()
        => GetRequestedPieces().Values.Sum();

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
}
