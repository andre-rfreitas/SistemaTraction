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
        var model = await context.DtfModels
            .FirstOrDefaultAsync(m => m.Id == request.DtfModelId && !m.IsDeleted, cancellationToken)
            ?? throw new DomainException("Modelo DTF não encontrado.");

        var stockItem = await context.DtfStockItems
            .FirstOrDefaultAsync(i => i.DtfModelId == request.DtfModelId && !i.IsDeleted, cancellationToken);

        if (stockItem is null)
        {
            stockItem = DtfStockItem.Create(request.DtfModelId);
            context.DtfStockItems.Add(stockItem);
        }

        DtfStockMovement movement;
        if (request.Type == DtfMovementType.Entrada)
        {
            var stamps = request.Quantity * model.StampsPerSheet;
            movement = stockItem.AddMovement(
                DtfMovementType.Entrada, stamps, request.Reason, sheetCount: request.Quantity);
        }
        else
        {
            movement = stockItem.AddMovement(request.Type, request.Quantity, request.Reason);
        }

        context.DtfStockMovements.Add(movement);

        await context.SaveChangesAsync(cancellationToken);

        return movement.Id;
    }
}
