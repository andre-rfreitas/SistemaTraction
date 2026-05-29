using SistemaTraction.Domain.Common;

namespace SistemaTraction.Domain.Fabric;

public class FabricColor : BaseEntity
{
    public Guid FabricTypeId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? HexCode { get; private set; }

    // Navegação reversa para EF Core
    public FabricType? FabricType { get; private set; }

    // EF Core constructor
    private FabricColor() { }

    public static FabricColor Create(Guid fabricTypeId, string name, string? hexCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da cor não pode ser vazio.");

        return new FabricColor
        {
            FabricTypeId = fabricTypeId,
            Name = name.Trim(),
            HexCode = hexCode?.Trim()
        };
    }

    public void Update(string name, string? hexCode)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Nome da cor não pode ser vazio.");

        Name = name.Trim();
        HexCode = hexCode?.Trim();
        TouchUpdatedAt();
    }
}
