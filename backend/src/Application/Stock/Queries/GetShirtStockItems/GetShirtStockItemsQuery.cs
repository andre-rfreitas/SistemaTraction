using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStockItems;

public record GetShirtStockItemsQuery(string ShirtType = "Regular") : IRequest<List<ShirtStockItemDto>>;
