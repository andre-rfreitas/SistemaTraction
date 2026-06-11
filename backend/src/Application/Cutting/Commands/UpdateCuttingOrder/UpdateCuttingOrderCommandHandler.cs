using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Cutting.Commands.UpdateCuttingOrder;

public class UpdateCuttingOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateCuttingOrderCommand, Unit>
{
    public async Task<Unit> Handle(UpdateCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        var itemInputs = request.Items
            .Select(i => (i.FabricRollId, i.RequestedPieces))
            .ToList();

        order.UpdateItems(itemInputs);
        order.UpdateNotes(request.Notes);

        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
