using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;

public record UpdateSupplyTypeCommand(Guid Id, string Name, string Unit) : IRequest;
