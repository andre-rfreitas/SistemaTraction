namespace SistemaTraction.Application.Fabric.DTOs;

public record FabricColorDto(
    Guid Id,
    Guid FabricTypeId,
    string Name,
    string? HexCode
);
