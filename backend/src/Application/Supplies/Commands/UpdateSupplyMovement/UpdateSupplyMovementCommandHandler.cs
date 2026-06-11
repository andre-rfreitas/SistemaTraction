using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyMovement;

public class UpdateSupplyMovementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSupplyMovementCommand>
{
    public async Task Handle(UpdateSupplyMovementCommand request, CancellationToken cancellationToken)
    {
        var movement = await context.SupplyStockMovements
            .FirstOrDefaultAsync(m => m.Id == request.MovementId, cancellationToken)
            ?? throw new DomainException("Movimentação não encontrada.");

        movement.UpdateMetadata(
            request.SupplierName,
            request.SupplierPhone,
            request.OccurredAt,
            request.UnitPrice,
            request.TotalCost);

        await context.SaveChangesAsync(cancellationToken);
    }
}
