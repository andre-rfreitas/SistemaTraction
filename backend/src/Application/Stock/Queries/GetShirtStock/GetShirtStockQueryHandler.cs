using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStock;

public class GetShirtStockQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShirtStockQuery, ShirtStockGridDto>
{
    public async Task<ShirtStockGridDto> Handle(GetShirtStockQuery request, CancellationToken cancellationToken)
    {
        var sizesConfig = await context.AppConfigs
            .Where(c => c.Key == "sizes_available" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "P,M,G,G1,GG";

        var alertThresholdStr = await context.AppConfigs
            .Where(c => c.Key == "stock_alert_threshold" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "15";

        var sizes = sizesConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var alertThreshold = int.TryParse(alertThresholdStr, out var t) ? t : 15;

        var shirtType = Enum.TryParse<ShirtType>(request.ShirtType, out var parsed) ? parsed : ShirtType.Regular;

        var stockItems = await context.StockItems
            .Where(s => !s.IsDeleted && s.ShirtType == shirtType)
            .ToListAsync(cancellationToken);

        // Group by color
        var byColor = stockItems
            .GroupBy(s => new { s.FabricColorId, s.FabricColorName })
            .OrderBy(g => g.Key.FabricColorName)
            .ToList();

        var rows = byColor.Select(g =>
        {
            var quantities = sizes.ToDictionary(
                sz => sz,
                sz => g.FirstOrDefault(s => s.Size == sz)?.Quantity ?? 0);
            var total = quantities.Values.Sum();
            return new ShirtStockRowDto(g.Key.FabricColorId, g.Key.FabricColorName, quantities, total);
        }).ToList();

        var totalsPerSize = sizes.ToDictionary(
            sz => sz,
            sz => stockItems.Where(s => s.Size == sz).Sum(s => s.Quantity));

        var grandTotal = stockItems.Sum(s => s.Quantity);

        return new ShirtStockGridDto(sizes, rows, totalsPerSize, grandTotal, alertThreshold);
    }
}
