using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;

public class RegisterDtfMovementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterDtfMovementCommand, Guid>
{
    public async Task<Guid> Handle(RegisterDtfMovementCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await context.DtfStockItems
            .FirstOrDefaultAsync(i => i.DtfModelId == request.DtfModelId && !i.IsDeleted, cancellationToken);

        if (stockItem is null)
        {
            var modelExists = await context.DtfModels
                .AnyAsync(m => m.Id == request.DtfModelId && !m.IsDeleted, cancellationToken);

            if (!modelExists)
                throw new DomainException("Modelo DTF não encontrado.");

            stockItem = DtfStockItem.Create(request.DtfModelId);
            context.DtfStockItems.Add(stockItem);
        }

        var movement = stockItem.AddMovement(request.Type, request.Quantity, request.Reason);
        context.DtfStockMovements.Add(movement);

        await context.SaveChangesAsync(cancellationToken);

        return movement.Id;
    }
}
