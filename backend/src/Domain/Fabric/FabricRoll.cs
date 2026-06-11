using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Fabric;

public class FabricRoll : BaseEntity
{
    public Guid FabricTypeId { get; private set; }
    public Guid FabricColorId { get; private set; }
    public decimal WeightKg { get; private set; }
    public decimal PriceTotal { get; private set; }
    public decimal PricePerKgActual { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public FabricRollStatus Status { get; private set; }

    public FabricType? FabricType { get; private set; }
    public FabricColor? FabricColor { get; private set; }

    private FabricRoll() { }

    public static FabricRoll Create(Guid fabricTypeId, Guid fabricColorId, decimal weightKg, decimal priceTotal)
    {
        if (weightKg <= 0)
            throw new DomainException("Peso da bobina deve ser maior que zero.");

        if (priceTotal <= 0)
            throw new DomainException("Preço total da bobina deve ser maior que zero.");

        return new FabricRoll
        {
            FabricTypeId = fabricTypeId,
            FabricColorId = fabricColorId,
            WeightKg = weightKg,
            PriceTotal = priceTotal,
            PricePerKgActual = priceTotal / weightKg,
            ReceivedAt = DateTime.UtcNow,
            Status = FabricRollStatus.Available
        };
    }

    public void Update(Guid fabricTypeId, Guid fabricColorId, decimal weightKg, decimal priceTotal)
    {
        if (weightKg <= 0)
            throw new DomainException("Peso da bobina deve ser maior que zero.");

        if (priceTotal <= 0)
            throw new DomainException("Preço total da bobina deve ser maior que zero.");

        FabricTypeId = fabricTypeId;
        FabricColorId = fabricColorId;
        WeightKg = weightKg;
        PriceTotal = priceTotal;
        PricePerKgActual = priceTotal / weightKg;
        TouchUpdatedAt();
    }

    public void StartCutting()
    {
        if (Status != FabricRollStatus.Available)
            throw new DomainException("Apenas bobinas disponíveis podem ser enviadas ao corte.");

        Status = FabricRollStatus.InCutting;
        TouchUpdatedAt();
    }

    public void RevertToAvailable()
    {
        if (Status != FabricRollStatus.InCutting)
            throw new DomainException("Apenas bobinas em corte podem ser revertidas para disponível.");
        Status = FabricRollStatus.Available;
        TouchUpdatedAt();
    }

    public void MarkConsumed()
    {
        if (Status == FabricRollStatus.Consumed)
            throw new DomainException("Bobina já está marcada como consumida.");

        Status = FabricRollStatus.Consumed;
        TouchUpdatedAt();
    }
}
