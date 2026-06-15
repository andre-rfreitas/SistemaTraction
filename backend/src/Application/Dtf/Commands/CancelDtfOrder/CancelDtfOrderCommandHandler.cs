using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.CancelDtfOrder;

public class CancelDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CancelDtfOrderCommand>
{
    public async Task Handle(CancelDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.Cancel();
        await context.SaveChangesAsync(cancellationToken);
    }
}
