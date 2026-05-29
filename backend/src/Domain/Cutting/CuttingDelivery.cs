using System.Text.Json;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Cutting;

public class CuttingDelivery : BaseEntity
{
    public Guid CuttingOrderId { get; private set; }
    public string DeliveredPiecesJson { get; private set; } = "{}";
    public DateTime DeliveredAt { get; private set; }
    public decimal CuttingCostTotal { get; private set; }

    public CuttingOrder? CuttingOrder { get; private set; }

    private CuttingDelivery() { }

    public static CuttingDelivery Create(Guid cuttingOrderId, Dictionary<string, int> deliveredPieces, decimal cuttingCostTotal)
    {
        if (deliveredPieces.Values.Any(v => v < 0))
            throw new DomainException("Quantidades entregues não podem ser negativas.");

        if (deliveredPieces.Values.Sum() == 0)
            throw new DomainException("A entrega deve ter pelo menos uma peça.");

        if (cuttingCostTotal < 0)
            throw new DomainException("Custo total não pode ser negativo.");

        return new CuttingDelivery
        {
            CuttingOrderId = cuttingOrderId,
            DeliveredPiecesJson = JsonSerializer.Serialize(deliveredPieces),
            DeliveredAt = DateTime.UtcNow,
            CuttingCostTotal = cuttingCostTotal
        };
    }

    public Dictionary<string, int> GetDeliveredPieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(DeliveredPiecesJson) ?? [];

    public int GetTotalPieces()
        => GetDeliveredPieces().Values.Sum();
}
