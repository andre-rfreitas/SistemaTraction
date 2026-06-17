using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Commands.AdjustGenericProductStock;

public class AdjustGenericProductStockCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AdjustGenericProductStockCommand, Guid>
{
    public async Task<Guid> Handle(AdjustGenericProductStockCommand request, CancellationToken cancellationToken)
    {
        var product = await context.GenericProducts
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        var isSaida = request.AdjustmentType == "Saída";
        var delta = isSaida ? -request.Quantity : request.Quantity;

        if (isSaida)
        {
            product.UseFromStock(request.Quantity);
        }
        else
        {
            product.AddStock(request.Quantity);
        }

        var movement = GenericProductMovement.Create(
            product.Id,
            product.Name,
            delta,
            request.Reason,
            "Manual");
        
        context.GenericProductMovements.Add(movement);

        if (!isSaida && request.UnitCost > 0)
        {
            var totalCost = request.UnitCost * request.Quantity;
            var financialEntry = FinancialEntry.CreateExpense(
                FinancialCategories.Estoque,
                totalCost,
                $"Entrada de estoque: {product.Category!.Name} - {product.Name} ({request.Quantity} un. × R$ {request.UnitCost:N2})",
                referenceId: movement.Id,
                referenceType: "GenericProductMovement");
            context.FinancialEntries.Add(financialEntry);
        }

        await context.SaveChangesAsync(cancellationToken);

        return movement.Id;
    }
}
