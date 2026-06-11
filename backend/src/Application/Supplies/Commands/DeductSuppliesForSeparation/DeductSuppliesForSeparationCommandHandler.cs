using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Financial;
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
            .Include(i => i.SupplyType)
            .Where(i => ids.Contains(i.Id) && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var deductItem in itemsToDeduct)
        {
            var stockItem = stockItems.FirstOrDefault(i => i.Id == deductItem.SupplyStockItemId);
            if (stockItem is null) continue;

            var pricePerUnit = stockItem.SupplyType.PricePerUnit;
            var totalCost = pricePerUnit.HasValue ? pricePerUnit.Value * deductItem.Quantity : (decimal?)null;

            var movement = stockItem.AddMovement(
                SupplyMovementType.Saida,
                deductItem.Quantity,
                "Separação de pedidos",
                unitPrice: pricePerUnit,
                totalCost: totalCost);
            context.SupplyStockMovements.Add(movement);

            if (totalCost.HasValue && totalCost.Value > 0)
            {
                context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                    stockItem.SupplyType.Name,
                    totalCost.Value,
                    $"Separação: {stockItem.SupplyType.Name} ({deductItem.Quantity} {stockItem.SupplyType.Unit})",
                    movement.Id,
                    "SupplyMovement"));
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
