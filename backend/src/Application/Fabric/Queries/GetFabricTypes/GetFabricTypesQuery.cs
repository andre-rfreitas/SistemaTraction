using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypes;

public record GetFabricTypesQuery : IRequest<List<FabricTypeDto>>;
