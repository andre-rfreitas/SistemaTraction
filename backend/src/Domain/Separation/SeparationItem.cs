using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Domain.Separation;

public class SeparationItem : BaseEntity
{
    public Guid SeparationListId { get; private set; }
    public string Sku { get; private set; } = "";
    // Vazio para SKUs de formato legado (3 segmentos) ou sem mapeamento de Estampa configurado.
    public string Estampa { get; private set; } = "";
    public string Color { get; private set; } = "";
    public string Size { get; private set; } = "";
    public int Quantity { get; private set; }
    public Guid? DtfModelId { get; private set; }
    public int SortOrder { get; private set; }

    public SeparationList? SeparationList { get; private set; }
    public DtfModel? DtfModel { get; private set; }

    private SeparationItem() { }

    public static SeparationItem Create(
        Guid separationListId, string sku, string estampa, string color, string size, int quantity, int sortOrder)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Cor é obrigatória.");

        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        return new SeparationItem
        {
            SeparationListId = separationListId,
            Sku = sku?.Trim() ?? "",
            Estampa = estampa?.Trim() ?? "",
            Color = color.Trim(),
            Size = size.Trim().ToUpper(),
            Quantity = quantity,
            SortOrder = sortOrder
        };
    }

    public void Update(string sku, string estampa, string color, string size, int quantity)
    {
        if (string.IsNullOrWhiteSpace(color))
            throw new DomainException("Cor é obrigatória.");

        if (string.IsNullOrWhiteSpace(size))
            throw new DomainException("Tamanho é obrigatório.");

        if (quantity <= 0)
            throw new DomainException("Quantidade deve ser maior que zero.");

        Sku = sku?.Trim() ?? "";
        Estampa = estampa?.Trim() ?? "";
        Color = color.Trim();
        Size = size.Trim().ToUpper();
        Quantity = quantity;
        TouchUpdatedAt();
    }

    public void SetDtfModel(Guid? dtfModelId)
    {
        DtfModelId = dtfModelId;
        TouchUpdatedAt();
    }
}
