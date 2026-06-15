using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.ReceiveDtfOrder;

public class ReceiveDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReceiveDtfOrderCommand>
{
    public async Task Handle(ReceiveDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        var activeItems = order.Items.ToList();
        var modelIds = activeItems
            .Select(i => i.DtfModelId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();

        var models = await context.DtfModels
            .Where(m => modelIds.Contains(m.Id) && !m.IsDeleted)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        var stockItems = await context.DtfStockItems
            .Where(s => modelIds.Contains(s.DtfModelId) && !s.IsDeleted)
            .ToDictionaryAsync(s => s.DtfModelId, cancellationToken);

        foreach (var item in activeItems)
        {
            if (!item.DtfModelId.HasValue)
                throw new DomainException("Item do pedido não tem modelo associado.");

            if (!models.TryGetValue(item.DtfModelId.Value, out var model))
                throw new DomainException("Modelo DTF não encontrado para item do pedido.");

            if (!stockItems.TryGetValue(item.DtfModelId.Value, out var stockItem))
            {
                stockItem = DtfStockItem.Create(item.DtfModelId.Value);
                context.DtfStockItems.Add(stockItem);
                stockItems[item.DtfModelId.Value] = stockItem;
            }

            int stamps;
            try { stamps = checked(item.SheetQuantity * model.StampsPerSheet); }
            catch (OverflowException) { throw new DomainException("Quantidade de folhas muito alta para conversão em estampas."); }

            var movement = stockItem.AddMovement(
                DtfMovementType.Entrada,
                stamps,
                reason: $"Pedido DTF #{order.OrderNumber}",
                sheetCount: item.SheetQuantity);

            context.DtfStockMovements.Add(movement);
        }

        order.MarkReceived();
        await context.SaveChangesAsync(cancellationToken);
    }
}
