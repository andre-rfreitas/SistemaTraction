using MediatR;

namespace SistemaTraction.Application.Stock.Commands.DeleteShirtStockItem;

public record DeleteShirtStockItemCommand(Guid StockItemId) : IRequest<DeleteShirtStockItemResult>;

public record DeleteShirtStockItemResult(Guid StockItemId);
