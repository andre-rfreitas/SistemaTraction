using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.DeductSuppliesForSeparation;

public class DeductSuppliesForSeparationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeductSuppliesForSeparationCommand>
{
    public async Task Handle(DeductSuppliesForSeparationCommand request, CancellationToken cancellationToken)
    {
        var itemsToDeduct = request.Items.Where(i => i.Quantity > 0).ToList();
        if (itemsToDeduct.Count == 0) return;

        var ids = itemsToDeduct.Select(i => i.SupplyStockItemId).ToList();
        var stockItems = await context.SupplyStockItems
            .Where(i => ids.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var deductItem in itemsToDeduct)
        {
            var stockItem = stockItems.FirstOrDefault(i => i.Id == deductItem.SupplyStockItemId);
            if (stockItem is null) continue;

            var movement = stockItem.AddMovement(
                SupplyMovementType.Saida, deductItem.Quantity, "Separação de pedidos");
            context.SupplyStockMovements.Add(movement);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
