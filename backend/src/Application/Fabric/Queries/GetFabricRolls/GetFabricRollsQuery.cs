using MediatR;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricRolls;

public record GetFabricRollsQuery(string? Status = null) : IRequest<List<FabricRollDto>>;
