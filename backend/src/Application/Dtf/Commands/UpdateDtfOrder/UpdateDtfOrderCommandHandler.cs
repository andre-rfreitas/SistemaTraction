using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;

public class UpdateDtfOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDtfOrderCommand>
{
    public async Task Handle(UpdateDtfOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await context.DtfOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido DTF não encontrado.");

        order.UpdateNotes(request.Notes);

        var requestedModelIds = request.Items.Select(i => i.DtfModelId).ToHashSet();

        foreach (var existingItem in order.Items.ToList())
        {
            if (!requestedModelIds.Contains(existingItem.DtfModelId))
                order.RemoveItem(existingItem.DtfModelId);
        }

        foreach (var input in request.Items)
        {
            var existing = order.Items.FirstOrDefault(i => i.DtfModelId == input.DtfModelId);
            if (existing is not null)
                existing.UpdateSheetQuantity(input.SheetQuantity);
            else
                order.AddItem(input.DtfModelId, input.SheetQuantity);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
