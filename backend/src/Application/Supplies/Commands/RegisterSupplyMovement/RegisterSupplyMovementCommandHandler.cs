using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
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

        var type = stockItem.SupplyType;

        // For saídas, compute cost from pricePerUnit if not explicitly provided
        decimal? unitPrice = request.UnitPrice ?? (request.Type == SupplyMovementType.Saida ? type.PricePerUnit : null);
        decimal? totalCost = request.TotalCost ?? (unitPrice.HasValue ? unitPrice.Value * request.Quantity : null);

        var movement = stockItem.AddMovement(
            request.Type,
            request.Quantity,
            request.Reason,
            request.SupplierName,
            request.SupplierPhone,
            request.OccurredAt,
            unitPrice,
            totalCost);
        context.SupplyStockMovements.Add(movement);

        // Saída with known cost → auto-register financial expense, no confirmation needed
        if (request.Type == SupplyMovementType.Saida && totalCost.HasValue && totalCost.Value > 0)
        {
            var description = $"Saída: {type.Name} ({request.Quantity} {type.Unit})";
            if (!string.IsNullOrWhiteSpace(request.Reason))
                description += $" — {request.Reason}";

            context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                type.Name,
                totalCost.Value,
                description,
                movement.Id,
                "SupplyMovement"));
        }

        await context.SaveChangesAsync(cancellationToken);

        var isEntry = request.Type == SupplyMovementType.Entrada;
        var suggestedDescription = isEntry
            ? $"Compra: {type.Name} ({request.Quantity} {type.Unit})"
            : null;
        var suggestedAmount = isEntry ? totalCost : null;

        return new RegisterSupplyMovementResult(movement.Id, isEntry, suggestedDescription, suggestedAmount);
    }
}
