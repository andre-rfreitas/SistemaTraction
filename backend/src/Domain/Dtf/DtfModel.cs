using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Dtf;

public class DtfModel : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string SheetLabel { get; private set; } = string.Empty;
    public int StampsPerSheet { get; private set; }
    public decimal SheetCost { get; private set; }

    // EF Core constructor
    private DtfModel() { }

    public static DtfModel Create(string name, string sheetLabel, int stampsPerSheet, decimal sheetCost)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do modelo DTF não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(sheetLabel))
            throw new DomainException("Rótulo da folha não pode ser vazio.");

        if (stampsPerSheet <= 0)
            throw new DomainException("Quantidade de estampas por folha deve ser maior que zero.");

        if (sheetCost <= 0)
            throw new DomainException("Custo da folha deve ser maior que zero.");

        return new DtfModel
        {
            Name = name.Trim(),
            SheetLabel = sheetLabel.Trim(),
            StampsPerSheet = stampsPerSheet,
            SheetCost = sheetCost
        };
    }

    public void Update(string name, string sheetLabel, int stampsPerSheet, decimal sheetCost)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do modelo DTF não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(sheetLabel))
            throw new DomainException("Rótulo da folha não pode ser vazio.");

        if (stampsPerSheet <= 0)
            throw new DomainException("Quantidade de estampas por folha deve ser maior que zero.");

        if (sheetCost <= 0)
            throw new DomainException("Custo da folha deve ser maior que zero.");

        Name = name.Trim();
        SheetLabel = sheetLabel.Trim();
        StampsPerSheet = stampsPerSheet;
        SheetCost = sheetCost;
        TouchUpdatedAt();
    }
}
