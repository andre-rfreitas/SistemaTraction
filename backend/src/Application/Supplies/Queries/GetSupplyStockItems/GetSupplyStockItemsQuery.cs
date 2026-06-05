using MediatR;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyStockItems;

public record GetSupplyStockItemsQuery : IRequest<List<SupplyStockItemDto>>;
