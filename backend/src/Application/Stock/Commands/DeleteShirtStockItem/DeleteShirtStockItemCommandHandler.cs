using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Stock;
using ShirtTypeEnum = SistemaTraction.Domain.Stock.ShirtType;

namespace SistemaTraction.Application.Stock.Commands.DeleteShirtStockItem;

public class DeleteShirtStockItemCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteShirtStockItemCommand, DeleteShirtStockItemResult>
{
    public async Task<DeleteShirtStockItemResult> Handle(
        DeleteShirtStockItemCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await context.StockItems
            .FirstOrDefaultAsync(s => s.Id == request.StockItemId && !s.IsDeleted, cancellationToken)
            ?? throw new DomainException("Item de estoque não encontrado.");

        // Se ainda há quantidade, registra movimento de saída total para rastreabilidade
        if (stockItem.Quantity > 0)
        {
            var movement = ShirtStockMovement.Create(
                stockItem.Id,
                stockItem.FabricColorId,
                stockItem.FabricColorName,
                stockItem.Size,
                -stockItem.Quantity,
                "Exclusão manual de item",
                "Manual",
                shirtType: stockItem.ShirtType);
            context.ShirtStockMovements.Add(movement);
        }

        stockItem.MarkAsDeleted();
        stockItem.TouchUpdatedAt();

        await context.SaveChangesAsync(cancellationToken);

        return new DeleteShirtStockItemResult(request.StockItemId);
    }
}
