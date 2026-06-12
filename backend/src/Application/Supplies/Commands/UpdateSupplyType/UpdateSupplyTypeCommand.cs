using MediatR;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;

public record UpdateSupplyTypeCommand(
    Guid Id,
    string Name,
    string Unit,
    decimal? PricePerUnit,
    YieldBasis? YieldBasis = null,
    decimal? YieldQuantity = null,
    string? YieldProductName = null
) : IRequest;
