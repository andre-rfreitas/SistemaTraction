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
            .Include(o => o.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        order.MarkSent();
        order.FabricRoll!.StartCutting();

        await context.SaveChangesAsync(cancellationToken);

        return new SendCuttingOrderResult(true, null, null);
    }
}
