namespace SistemaTraction.Application.Sewing.DTOs;

public record SewerProductTypeDto(Guid Id, string Name, decimal PriceDefault, decimal PriceG1);

public record SewerDto(Guid Id, string Name, string? Phone, bool IsActive, List<SewerProductTypeDto> ProductTypes);
