using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;

public record DeleteFabricColorCommand(Guid FabricTypeId, Guid ColorId) : IRequest;
