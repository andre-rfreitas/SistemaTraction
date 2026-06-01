using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingOrderById;

public class GetCuttingOrderByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCuttingOrderByIdQuery, CuttingOrderDto?>
{
    public async Task<CuttingOrderDto?> Handle(GetCuttingOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await context.CuttingOrders
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricColor)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);

        if (o is null) return null;

        var delivery = await context.CuttingDeliveries
            .FirstOrDefaultAsync(d => d.CuttingOrderId == o.Id, cancellationToken);

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
    }
}
