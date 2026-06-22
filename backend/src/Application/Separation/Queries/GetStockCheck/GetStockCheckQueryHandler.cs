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
            .GroupBy(i => (
                ModelCode: (i.Sku.Split('-').FirstOrDefault() ?? "REG").ToUpper(),
                Color: i.Color, 
                Size: i.Size.ToUpper()
            ))
            .ToList();

        var shirtChecks = new List<ShirtStockCheckDto>();
        var shirtOk = true;

        foreach (var group in shirtGroups)
        {
            var needed = group.Sum(i => i.Quantity);
            var stockItem = await context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.ModelCode == group.Key.ModelCode &&
                    s.FabricColorName.ToLower() == group.Key.Color.ToLower() &&
                    s.Size == group.Key.Size &&
                    !s.IsDeleted,
                    cancellationToken);

            // Fallback para itens antigos sem model code exato
            if (stockItem is null)
            {
                stockItem = await context.StockItems
                    .FirstOrDefaultAsync(s =>
                        s.FabricColorName.ToLower() == group.Key.Color.ToLower() &&
                        s.Size == group.Key.Size &&
                        !s.IsDeleted,
                        cancellationToken);
            }

            var available = stockItem?.Quantity ?? 0;
            var ok = available >= needed;
            if (!ok) shirtOk = false;

            shirtChecks.Add(new ShirtStockCheckDto(group.Key.ModelCode, group.Key.Color, group.Key.Size, needed, available, ok));
        }

        return new StockCheckResultDto(shirtChecks, shirtOk);
    }
}
