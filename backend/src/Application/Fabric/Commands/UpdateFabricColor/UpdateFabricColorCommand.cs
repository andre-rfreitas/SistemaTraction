using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public record UpdateFabricColorCommand(Guid Id, string Name, string? HexCode) : IRequest;
