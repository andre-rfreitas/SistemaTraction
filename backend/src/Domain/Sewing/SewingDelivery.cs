using System.Text.Json;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Domain.Sewing;

public class SewingDelivery : BaseEntity
{
    public Guid CuttingOrderId { get; private set; }
    public string GoodPiecesJson { get; private set; } = "{}";
    public string DefectivePiecesJson { get; private set; } = "{}";
    public DateTime DeliveredAt { get; private set; }
    public decimal SewingCostTotal { get; private set; }
    public decimal DefectCostTotal { get; private set; }
    public bool IsPartial { get; private set; }

    public CuttingOrder? CuttingOrder { get; private set; }

    private SewingDelivery() { }

    public static SewingDelivery Create(
        Guid cuttingOrderId,
        Dictionary<string, int> goodPieces,
        Dictionary<string, int> defectivePieces,
        decimal sewingCostTotal,
        decimal defectCostTotal,
        bool isPartial = false)
    {
        if (goodPieces.Values.Any(v => v < 0))
            throw new DomainException("Quantidades de peças boas não podem ser negativas.");

        if (goodPieces.Values.Sum() == 0)
            throw new DomainException("A entrega deve ter pelo menos uma peça boa.");

        if (defectivePieces.Values.Any(v => v < 0))
            throw new DomainException("Quantidades defeituosas não podem ser negativas.");

        if (sewingCostTotal < 0)
            throw new DomainException("Custo de costura não pode ser negativo.");

        if (defectCostTotal < 0)
            throw new DomainException("Custo de defeitos não pode ser negativo.");

        return new SewingDelivery
        {
            CuttingOrderId = cuttingOrderId,
            GoodPiecesJson = JsonSerializer.Serialize(goodPieces),
            DefectivePiecesJson = JsonSerializer.Serialize(defectivePieces),
            DeliveredAt = DateTime.UtcNow,
            SewingCostTotal = sewingCostTotal,
            DefectCostTotal = defectCostTotal,
            IsPartial = isPartial
        };
    }

    public Dictionary<string, int> GetGoodPieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(GoodPiecesJson) ?? [];

    public Dictionary<string, int> GetDefectivePieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(DefectivePiecesJson) ?? [];

    public int GetTotalGoodPieces() => GetGoodPieces().Values.Sum();
    public int GetTotalDefectivePieces() => GetDefectivePieces().Values.Sum();
}
