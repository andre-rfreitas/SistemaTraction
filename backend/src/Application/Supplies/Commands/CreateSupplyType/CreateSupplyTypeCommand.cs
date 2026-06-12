using MediatR;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.CreateSupplyType;

public record CreateSupplyTypeCommand(
    string Name,
    string Unit,
    decimal? PricePerUnit,
    YieldBasis? YieldBasis = null,
    decimal? YieldQuantity = null,
    string? YieldProductName = null
) : IRequest<Guid>;
