namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyTypeDto(Guid Id, string Name, string Unit, decimal? PricePerUnit);
