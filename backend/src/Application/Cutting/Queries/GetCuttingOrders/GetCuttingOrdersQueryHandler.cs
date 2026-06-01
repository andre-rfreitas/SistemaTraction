using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingOrders;

public class GetCuttingOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCuttingOrdersQuery, List<CuttingOrderDto>>
{
    public async Task<List<CuttingOrderDto>> Handle(GetCuttingOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.CuttingOrders
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricColor)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<CuttingOrderStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var orders = await query.OrderByDescending(o => o.OrderNumber).ToListAsync(cancellationToken);

        var orderIds = orders.Select(o => o.Id).ToList();
        var deliveries = await context.CuttingDeliveries
            .Where(d => orderIds.Contains(d.CuttingOrderId))
            .ToListAsync(cancellationToken);
        var deliveryMap = deliveries.ToDictionary(d => d.CuttingOrderId);

        return orders.Select(o =>
        {
            deliveryMap.TryGetValue(o.Id, out var delivery);
            return new CuttingOrderDto(
                o.Id,
                o.OrderNumber,
                o.FabricRollId,
                o.FabricRoll!.FabricType!.Name,
                o.FabricRoll!.FabricType!.Variation,
                o.FabricRoll!.FabricColor!.Name,
                o.FabricRoll!.FabricColor!.HexCode,
                o.FabricRoll!.WeightKg,
                o.GetRequestedPieces(),
                delivery?.GetDeliveredPieces(),
                o.GetTotalPieces(),
                o.Status.ToString(),
                o.SentAt,
                o.Notes,
                o.CreatedAt
            );
        }).ToList();
    }
}
