using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricRollById;

public record GetFabricRollByIdQuery(Guid Id) : IRequest<FabricRollDto?>;
