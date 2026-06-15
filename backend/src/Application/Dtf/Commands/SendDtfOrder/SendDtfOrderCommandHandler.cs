using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.SendDtfOrder;

public class SendDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SendDtfOrderCommand>
{
    public async Task Handle(SendDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.MarkSent();
        await context.SaveChangesAsync(cancellationToken);
    }
}
