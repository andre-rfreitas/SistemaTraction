using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.RegisterSupplyMovement;

public class RegisterSupplyMovementCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterSupplyMovementCommand, RegisterSupplyMovementResult>
{
    public async Task<RegisterSupplyMovementResult> Handle(
        RegisterSupplyMovementCommand request, CancellationToken cancellationToken)
    {
        var stockItem = await context.SupplyStockItems
            .Include(i => i.SupplyType)
            .FirstOrDefaultAsync(i => i.Id == request.SupplyStockItemId && !i.IsDeleted, cancellationToken)
            ?? throw new DomainException("Item de estoque não encontrado.");

        var movement = stockItem.AddMovement(request.Type, request.Quantity, request.Reason);
        context.SupplyStockMovements.Add(movement);
        await context.SaveChangesAsync(cancellationToken);

        var isEntry = request.Type == SupplyMovementType.Entrada;
        var suggestedDescription = isEntry
            ? $"Compra: {stockItem.SupplyType.Name} ({request.Quantity} {stockItem.SupplyType.Unit})"
            : null;

        return new RegisterSupplyMovementResult(movement.Id, isEntry, suggestedDescription);
    }
}
