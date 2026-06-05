using MediatR;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyOrderConfigs;

public record GetSupplyOrderConfigsQuery : IRequest<List<SupplyOrderConfigDto>>;
