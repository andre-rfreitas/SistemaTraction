using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.DTOs;

public record SupplyTypeDto(
    Guid Id,
    string Name,
    string Unit,
    decimal? PricePerUnit,
    YieldBasis YieldBasis,
    decimal? YieldQuantity,
    string? YieldProductName);
