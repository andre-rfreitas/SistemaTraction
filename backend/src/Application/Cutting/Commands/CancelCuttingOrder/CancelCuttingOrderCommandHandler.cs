using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;

public class CancelCuttingOrderCommandHandler(IApplicationDbContext context, ISender sender)
    : IRequestHandler<CancelCuttingOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        if (order.Status == CuttingOrderStatus.Delivered)
        {
            await CancelWithFinancialReversal(order, "CuttingDelivery", cancellationToken);
            return Unit.Value;
        }

        if (order.Status == CuttingOrderStatus.SewingDelivered)
        {
            await CancelWithFinancialReversal(order, "SewingDelivery", cancellationToken);
            return Unit.Value;
        }

        if (order.Status == CuttingOrderStatus.SentToCutter)
        {
            foreach (var item in order.Items)
                item.FabricRoll?.RevertToAvailable();
        }

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private async Task CancelWithFinancialReversal(CuttingOrder order, string referenceType, CancellationToken cancellationToken)
    {
        Guid? deliveryId = referenceType == "CuttingDelivery"
            ? (await context.CuttingDeliveries
                .FirstOrDefaultAsync(d => d.CuttingOrderId == order.Id, cancellationToken))?.Id
            : (await context.SewingDeliveries
                .FirstOrDefaultAsync(s => s.CuttingOrderId == order.Id && !s.IsDeleted, cancellationToken))?.Id;

        if (deliveryId is not null)
        {
            var entry = await context.FinancialEntries
                .FirstOrDefaultAsync(e => e.ReferenceType == referenceType
                                       && e.ReferenceId == deliveryId
                                       && !e.IsReversal, cancellationToken);

            if (entry is not null)
            {
                await sender.Send(new ReverseFinancialEntryCommand(entry.Id), cancellationToken);
                return;
            }
        }

        order.CancelDelivered();
        await context.SaveChangesAsync(cancellationToken);
    }
}
