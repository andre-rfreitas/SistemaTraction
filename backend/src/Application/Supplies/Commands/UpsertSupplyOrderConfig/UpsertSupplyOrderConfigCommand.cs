using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.UpsertSupplyOrderConfig;

public record UpsertSupplyOrderConfigCommand(Guid SupplyTypeId, int QuantityPerOrder) : IRequest;
