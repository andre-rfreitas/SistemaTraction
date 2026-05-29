using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public record CreateFabricColorCommand(Guid FabricTypeId, string Name, string? HexCode) : IRequest<Guid>;
