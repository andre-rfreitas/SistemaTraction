using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfOrderItem : BaseEntity
{
    public Guid DtfOrderId { get; private set; }
    public Guid? DtfModelId { get; private set; }
    public int SheetQuantity { get; private set; }

    private DtfOrderItem() { }

    public static DtfOrderItem Create(Guid dtfOrderId, Guid dtfModelId, int sheetQuantity)
    {
        if (sheetQuantity <= 0)
            throw new DomainException("Quantidade de folhas deve ser maior que zero.");

        return new DtfOrderItem
        {
            DtfOrderId = dtfOrderId,
            DtfModelId = dtfModelId,
            SheetQuantity = sheetQuantity,
        };
    }

    public void UpdateSheetQuantity(int sheetQuantity)
    {
        if (sheetQuantity <= 0)
            throw new DomainException("Quantidade de folhas deve ser maior que zero.");
        SheetQuantity = sheetQuantity;
        TouchUpdatedAt();
    }
}
