namespace SistemaTraction.Application.Fabric.DTOs;

public record FabricRollDto(
    Guid Id,
    Guid FabricTypeId,
    string FabricTypeName,
    string FabricTypeVariation,
    decimal FabricTypePricePerKg,
    Guid FabricColorId,
    string FabricColorName,
    string? FabricColorHexCode,
    decimal WeightKg,
    decimal PriceTotal,
    decimal PricePerKgActual,
    DateTime ReceivedAt,
    string Status,
    DateTime CreatedAt
);
