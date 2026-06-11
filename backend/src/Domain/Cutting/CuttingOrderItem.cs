using System.Text.Json;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Domain.Cutting;

public class CuttingOrderItem : BaseEntity
{
    public Guid CuttingOrderId { get; private set; }
    public Guid FabricRollId { get; private set; }
    public string RequestedPiecesJson { get; private set; } = "{}";

    public FabricRoll? FabricRoll { get; private set; }

    private CuttingOrderItem() { }

    public static CuttingOrderItem Create(Guid cuttingOrderId, Guid fabricRollId, Dictionary<string, int> pieces)
        => new()
        {
            CuttingOrderId = cuttingOrderId,
            FabricRollId = fabricRollId,
            RequestedPiecesJson = JsonSerializer.Serialize(pieces)
        };

    public void UpdatePieces(Dictionary<string, int> pieces)
    {
        if (pieces.Values.Any(v => v < 0))
            throw new DomainException("Quantidades não podem ser negativas.");
        if (pieces.Values.Sum() == 0)
            throw new DomainException("Cada bobina deve ter pelo menos uma peça.");
        RequestedPiecesJson = JsonSerializer.Serialize(pieces);
        TouchUpdatedAt();
    }

    public Dictionary<string, int> GetRequestedPieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(RequestedPiecesJson) ?? [];

    public int GetTotalPieces() => GetRequestedPieces().Values.Sum();
}
