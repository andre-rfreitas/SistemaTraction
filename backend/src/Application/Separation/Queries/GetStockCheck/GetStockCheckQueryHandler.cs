using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Separation.Queries.GetStockCheck;

public class GetStockCheckQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetStockCheckQuery, StockCheckResultDto>
{
    public async Task<StockCheckResultDto> Handle(
        GetStockCheckQuery request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.SeparationListId && !l.IsDeleted, cancellationToken)
            ?? throw new DomainException("Lista não encontrada.");

        var items = await context.SeparationItems
            .Where(i => i.SeparationListId == request.SeparationListId && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        // ── Shirt stock check ──────────────────────────────────────────────────
        var shirtGroups = items
            .GroupBy(i => (Color: i.Color, Size: i.Size.ToUpper()))
            .ToList();

        var shirtChecks = new List<ShirtStockCheckDto>();
        var shirtOk = true;

        foreach (var group in shirtGroups)
        {
            var needed = group.Sum(i => i.Quantity);
            var stockItem = await context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.FabricColorName.ToLower() == group.Key.Color.ToLower() &&
                    s.Size == group.Key.Size &&
                    !s.IsDeleted,
                    cancellationToken);

            var available = stockItem?.Quantity ?? 0;
            var ok = available >= needed;
            if (!ok) shirtOk = false;

            shirtChecks.Add(new ShirtStockCheckDto(group.Key.Color, group.Key.Size, needed, available, ok));
        }

        // ── DTF stock check ────────────────────────────────────────────────────
        var dtfGroups = items
            .Where(i => i.DtfModelId.HasValue)
            .GroupBy(i => i.DtfModelId!.Value)
            .ToList();

        var dtfChecks = new List<DtfStockCheckDto>();
        var totalDtfCost = 0m;

        foreach (var group in dtfGroups)
        {
            var modelId = group.Key;
            var needed = group.Sum(i => i.Quantity);

            var model = await context.DtfModels
                .FirstOrDefaultAsync(m => m.Id == modelId && !m.IsDeleted, cancellationToken);

            if (model is null) continue;

            var stockItem = await context.DtfStockItems
                .FirstOrDefaultAsync(s => s.DtfModelId == modelId && !s.IsDeleted, cancellationToken);

            var available = stockItem?.CurrentQuantity ?? 0;
            var fromStock = Math.Min(available, needed);
            var deficit = needed - available;
            var sheetsToOrder = deficit > 0 ? (int)Math.Ceiling((double)deficit / model.StampsPerSheet) : 0;
            var stampsFromSheets = sheetsToOrder * model.StampsPerSheet;
            var surplus = stampsFromSheets - deficit;
            var orderCost = sheetsToOrder * model.SheetCost;
            totalDtfCost += orderCost;

            dtfChecks.Add(new DtfStockCheckDto(
                model.Id, model.Name, model.SheetLabel, model.StampsPerSheet, model.SheetCost,
                needed, available, sheetsToOrder == 0, sheetsToOrder,
                stampsFromSheets, surplus, orderCost));
        }

        return new StockCheckResultDto(shirtChecks, dtfChecks, totalDtfCost, shirtOk);
    }
}
