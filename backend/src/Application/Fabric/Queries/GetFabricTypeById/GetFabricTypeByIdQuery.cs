using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;

public record GetFabricTypeByIdQuery(Guid Id) : IRequest<FabricTypeDto?>;
