using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Fabric;

public class FabricType : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Variation { get; private set; } = string.Empty;
    public decimal PricePerKg { get; private set; }
    public decimal AverageKgPerRoll { get; private set; }
    public int? AveragePiecesPerRoll { get; private set; }

    private readonly List<FabricColor> _colors = [];
    public IReadOnlyCollection<FabricColor> Colors => _colors.AsReadOnly();

    // EF Core constructor
    private FabricType() { }

    public static FabricType Create(string name, string variation, decimal pricePerKg, decimal averageKgPerRoll, int? averagePiecesPerRoll = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de tecido não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(variation))
            throw new DomainException("Variação do tipo de tecido não pode ser vazia.");

        if (pricePerKg <= 0)
            throw new DomainException("Preço por kg deve ser maior que zero.");

        if (averageKgPerRoll <= 0)
            throw new DomainException("Média de kg por bobina deve ser maior que zero.");

        return new FabricType
        {
            Name = name.Trim(),
            Variation = variation.Trim(),
            PricePerKg = pricePerKg,
            AverageKgPerRoll = averageKgPerRoll,
            AveragePiecesPerRoll = averagePiecesPerRoll
        };
    }

    public void Update(string name, string variation, decimal pricePerKg, decimal averageKgPerRoll, int? averagePiecesPerRoll)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome do tipo de tecido não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(variation))
            throw new DomainException("Variação do tipo de tecido não pode ser vazia.");

        if (pricePerKg <= 0)
            throw new DomainException("Preço por kg deve ser maior que zero.");

        if (averageKgPerRoll <= 0)
            throw new DomainException("Média de kg por bobina deve ser maior que zero.");

        Name = name.Trim();
        Variation = variation.Trim();
        PricePerKg = pricePerKg;
        AverageKgPerRoll = averageKgPerRoll;
        AveragePiecesPerRoll = averagePiecesPerRoll;
        TouchUpdatedAt();
    }

    public FabricColor AddColor(string name, string? hexCode = null)
    {
        if (_colors.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase) && !c.IsDeleted))
            throw new DomainException($"Cor '{name}' já existe neste tipo de tecido.");

        var color = FabricColor.Create(Id, name, hexCode);
        _colors.Add(color);
        TouchUpdatedAt();
        return color;
    }

    public void RemoveColor(Guid colorId)
    {
        var color = _colors.FirstOrDefault(c => c.Id == colorId && !c.IsDeleted)
            ?? throw new DomainException("Cor não encontrada.");

        color.MarkAsDeleted();
        TouchUpdatedAt();
    }
}
