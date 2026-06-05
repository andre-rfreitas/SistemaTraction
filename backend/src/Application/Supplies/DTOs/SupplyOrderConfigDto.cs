namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyOrderConfigDto(Guid SupplyTypeId, string Name, string Unit, int QuantityPerOrder);
