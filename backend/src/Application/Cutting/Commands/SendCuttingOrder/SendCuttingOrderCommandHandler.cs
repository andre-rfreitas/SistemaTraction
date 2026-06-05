using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Cutting.Commands.SendCuttingOrder;

public class SendCuttingOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SendCuttingOrderCommand, SendCuttingOrderResult>
{
    public async Task<SendCuttingOrderResult> Handle(SendCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        order.MarkSent();

        foreach (var item in order.Items)
            item.FabricRoll!.StartCutting();

        await context.SaveChangesAsync(cancellationToken);

        return new SendCuttingOrderResult(true, null, null);
    }
}
