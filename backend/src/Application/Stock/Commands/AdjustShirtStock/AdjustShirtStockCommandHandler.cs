using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Commands.AdjustShirtStock;

public class AdjustShirtStockCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AdjustShirtStockCommand, AdjustShirtStockResult>
{
    public async Task<AdjustShirtStockResult> Handle(
        AdjustShirtStockCommand request, CancellationToken cancellationToken)
    {
        var color = await context.FabricColors
            .Include(c => c.FabricType)
            .FirstOrDefaultAsync(c => c.Id == request.FabricColorId && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Cor não encontrada.");

        var size = request.Size.Trim().ToUpper();
        var isSaida = request.AdjustmentType == "Saída";
        var delta = isSaida ? -request.Quantity : request.Quantity;
        var modelCode = string.IsNullOrWhiteSpace(request.ModelCode) ? "REG" : request.ModelCode.Trim().ToUpper();

        var stockItem = await context.StockItems
            .FirstOrDefaultAsync(
                s => s.FabricColorId == request.FabricColorId && s.Size == size && s.ModelCode == modelCode && !s.IsDeleted,
                cancellationToken);

        if (isSaida)
        {
            var available = stockItem?.Quantity ?? 0;
            if (request.Quantity > available)
                throw new DomainException(
                    $"Estoque insuficiente para {color.Name} {size}. Disponível: {available}, solicitado: {request.Quantity}.");

            stockItem!.UseFromStock(request.Quantity);
        }
        else
        {
            if (stockItem is null)
            {
                stockItem = StockItem.Create(
                    request.FabricColorId,
                    color.Name,
                    color.FabricType!.Name,
                    color.FabricType!.Variation,
                    size,
                    request.Quantity,
                    modelCode);
                context.StockItems.Add(stockItem);
            }
            else
            {
                stockItem.AddStock(request.Quantity);
            }
        }

        var movement = ShirtStockMovement.Create(
            stockItem.Id,
            request.FabricColorId,
            color.Name,
            size,
            delta,
            request.Reason,
            "Manual",
            modelCode: modelCode);
        context.ShirtStockMovements.Add(movement);

        // Lançamento financeiro automático: quando há valor unitário informado em uma entrada
        if (!isSaida && request.UnitCost.GetValueOrDefault() > 0)
        {
            var totalCost = request.UnitCost!.Value * request.Quantity;
            var financialEntry = FinancialEntry.CreateExpense(
                FinancialCategories.Estoque,
                totalCost,
                $"Entrada de estoque: {color.Name} {size} {modelCode} ({request.Quantity} un. × R$ {request.UnitCost:N2})",
                referenceId: movement.Id,
                referenceType: "ShirtStockMovement");
            context.FinancialEntries.Add(financialEntry);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new AdjustShirtStockResult(movement.Id, color.Name, size, delta, stockItem.Quantity);
    }
}
