using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Stock;
using ShirtTypeEnum = SistemaTraction.Domain.Stock.ShirtType;

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
        var shirtType = Enum.TryParse<ShirtTypeEnum>(request.ShirtType, out var parsed) ? parsed : ShirtTypeEnum.Regular;

        var stockItem = await context.StockItems
            .FirstOrDefaultAsync(
                s => s.FabricColorId == request.FabricColorId && s.Size == size && s.ShirtType == shirtType && !s.IsDeleted,
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
                    shirtType);
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
            shirtType: shirtType);
        context.ShirtStockMovements.Add(movement);

        await context.SaveChangesAsync(cancellationToken);

        return new AdjustShirtStockResult(movement.Id, color.Name, size, delta, stockItem.Quantity);
    }
}
