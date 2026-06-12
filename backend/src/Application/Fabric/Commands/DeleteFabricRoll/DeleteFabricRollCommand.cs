using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricRoll;

public record DeleteFabricRollCommand(Guid Id) : IRequest<Unit>;
