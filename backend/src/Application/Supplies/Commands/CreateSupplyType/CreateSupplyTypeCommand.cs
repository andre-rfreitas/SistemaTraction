using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.CreateSupplyType;

public record CreateSupplyTypeCommand(string Name, string Unit, decimal? PricePerUnit) : IRequest<Guid>;
