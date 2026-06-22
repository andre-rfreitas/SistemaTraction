using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStockItems;

public record GetShirtStockItemsQuery(string ModelCode = "REG") : IRequest<List<ShirtStockItemDto>>;
