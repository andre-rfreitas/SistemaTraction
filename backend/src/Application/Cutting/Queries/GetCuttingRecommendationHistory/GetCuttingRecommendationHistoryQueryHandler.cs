using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingRecommendationHistory;

public class GetCuttingRecommendationHistoryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCuttingRecommendationHistoryQuery, List<CuttingRecommendationHistoryItemDto>>
{
    public async Task<List<CuttingRecommendationHistoryItemDto>> Handle(
        GetCuttingRecommendationHistoryQuery request, CancellationToken cancellationToken)
    {
        var orders = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricColor)
            .Where(o => !o.IsDeleted && o.RecommendedPiecesJson != null)
            .OrderByDescending(o => o.CreatedAt)
            .Take(request.Take)
            .ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();

        var deliveries = await context.CuttingDeliveries
            .Where(d => orderIds.Contains(d.CuttingOrderId))
            .ToListAsync(cancellationToken);

        var deliveryByOrder = deliveries.ToDictionary(d => d.CuttingOrderId);

        return orders.Select(o =>
        {
            deliveryByOrder.TryGetValue(o.Id, out var delivery);
            var firstColor = o.Items.FirstOrDefault()?.FabricRoll?.FabricColor?.Name ?? "";
            return new CuttingRecommendationHistoryItemDto(
                CuttingOrderId: o.Id,
                OrderNumber: o.OrderNumber,
                FabricColorName: firstColor,
                CreatedAt: o.CreatedAt,
                DaysUsed: o.RecommendationDays ?? 0,
                BasedOnOrders: o.RecommendationBasedOnOrders ?? 0,
                RecommendedPieces: o.GetRecommendedPieces() ?? [],
                RequestedPieces: o.GetRequestedPieces(),
                ActualDeliveredPieces: delivery?.GetDeliveredPieces()
            );
        }).ToList();
    }
}
