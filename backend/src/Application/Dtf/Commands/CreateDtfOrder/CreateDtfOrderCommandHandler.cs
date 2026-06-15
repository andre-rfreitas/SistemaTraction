using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfOrder;

public class CreateDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDtfOrderCommand, Guid>
{
    public async Task<Guid> Handle(CreateDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var orderNumber = await context.DtfOrders
            .Where(o => !o.IsDeleted)
            .MaxAsync(o => (int?)o.OrderNumber, cancellationToken) ?? 0;
        orderNumber++;

        var items = request.Items
            .Select(i => (i.DtfModelId, i.SheetQuantity))
            .ToList();

        var order = DtfOrder.Create(orderNumber, items, request.Notes);
        context.DtfOrders.Add(order);

        await context.SaveChangesAsync(cancellationToken);
        return order.Id;
    }
}
