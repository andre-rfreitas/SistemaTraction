namespace SistemaTraction.Application.Fabric.DTOs;

public record FabricTypeDto(
    Guid Id,
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll,
    IReadOnlyCollection<FabricColorDto> Colors
);
