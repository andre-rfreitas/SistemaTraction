using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.DeleteSupplyType;

public record DeleteSupplyTypeCommand(Guid Id) : IRequest;
