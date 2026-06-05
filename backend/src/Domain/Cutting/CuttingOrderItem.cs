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

    public Dictionary<string, int> GetRequestedPieces()
        => JsonSerializer.Deserialize<Dictionary<string, int>>(RequestedPiecesJson) ?? [];

    public int GetTotalPieces() => GetRequestedPieces().Values.Sum();
}
