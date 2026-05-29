using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricType;

public record DeleteFabricTypeCommand(Guid Id) : IRequest;
