using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendation;

public class GetCuttingRecommendationQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCuttingRecommendationQuery, CuttingRecommendationDto>
{
    public async Task<CuttingRecommendationDto> Handle(
        GetCuttingRecommendationQuery request, CancellationToken cancellationToken)
    {
        var roll = await context.FabricRolls
            .Include(r => r.FabricColor)
            .FirstOrDefaultAsync(r => r.Id == request.FabricRollId && !r.IsDeleted, cancellationToken)
            ?? throw new DomainException("Bobina não encontrada.");

        var colorName = roll.FabricColor!.Name;
        var colorId = roll.FabricColorId;

        var daysBack = request.DaysBack
            ?? await GetRecommendationDaysAsync(cancellationToken);

        var cutoff = DateTime.UtcNow.AddDays(-daysBack);

        var confirmedListIds = await context.SeparationLists
            .Where(l => !l.IsDeleted
                && l.Status == SeparationListStatus.Confirmed
                && l.UploadedAt >= cutoff)
            .Select(l => l.Id)
            .ToListAsync(cancellationToken);

        var demandBySize = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        if (confirmedListIds.Count > 0)
        {
            var items = await context.SeparationItems
                .Where(i => !i.IsDeleted
                    && confirmedListIds.Contains(i.SeparationListId)
                    && i.Color == colorName)
                .GroupBy(i => i.Size)
                .Select(g => new { Size = g.Key, Total = g.Sum(i => i.Quantity) })
                .ToListAsync(cancellationToken);

            foreach (var item in items)
                demandBySize[item.Size] = item.Total;
        }

        var stockItems = await context.StockItems
            .Where(s => !s.IsDeleted && s.FabricColorId == colorId)
            .ToListAsync(cancellationToken);

        var currentStockBySize = stockItems.ToDictionary(
            s => s.Size,
            s => s.Quantity,
            StringComparer.OrdinalIgnoreCase);

        var sizesConfig = await context.AppConfigs
            .Where(c => c.Key == "sizes_available" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "P,M,G,G1,GG";

        var sizes = sizesConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);

        var recommendedPieces = new Dictionary<string, int>();
        foreach (var size in sizes)
        {
            var demand = demandBySize.GetValueOrDefault(size, 0);
            var stock = currentStockBySize.GetValueOrDefault(size, 0);
            recommendedPieces[size] = Math.Max(0, demand - stock);
        }

        return new CuttingRecommendationDto(
            ColorName: colorName,
            DaysUsed: daysBack,
            BasedOnOrders: confirmedListIds.Count,
            RecommendedPieces: recommendedPieces,
            DemandBySize: demandBySize,
            CurrentStockBySize: currentStockBySize,
            HasSufficientHistory: confirmedListIds.Count > 0
        );
    }

    private async Task<int> GetRecommendationDaysAsync(CancellationToken ct)
    {
        var value = await context.AppConfigs
            .Where(c => c.Key == "recommendation_days" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(ct);

        return int.TryParse(value, out var days) ? days : 30;
    }
}
