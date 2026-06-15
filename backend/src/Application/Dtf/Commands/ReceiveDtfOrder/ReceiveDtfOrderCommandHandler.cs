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

        foreach (var item in activeItems)
        {
            var model = await context.DtfModels
                .FirstOrDefaultAsync(m => m.Id == item.DtfModelId && !m.IsDeleted, cancellationToken)
                ?? throw new DomainException($"Modelo DTF não encontrado para item do pedido.");

            var stockItem = await context.DtfStockItems
                .FirstOrDefaultAsync(s => s.DtfModelId == item.DtfModelId && !s.IsDeleted, cancellationToken);

            if (stockItem is null)
            {
                stockItem = DtfStockItem.Create(item.DtfModelId);
                context.DtfStockItems.Add(stockItem);
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
