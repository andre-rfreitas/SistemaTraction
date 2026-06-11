using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Application.Cutting.Commands.CancelCuttingOrder;

public class CancelCuttingOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelCuttingOrderCommand, Unit>
{
    public async Task<Unit> Handle(CancelCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        if (order.Status == CuttingOrderStatus.SentToCutter)
        {
            foreach (var item in order.Items)
                item.FabricRoll?.RevertToAvailable();
        }

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
