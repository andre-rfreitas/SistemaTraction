using MediatR;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyTypes;

public record GetSupplyTypesQuery : IRequest<List<SupplyTypeDto>>;
