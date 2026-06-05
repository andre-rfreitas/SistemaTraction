using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Application.Cutting.Queries.GetCuttingOrders;

namespace SistemaTraction.Application.Cutting.Queries.GetCuttingOrderById;

public class GetCuttingOrderByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCuttingOrderByIdQuery, CuttingOrderDto?>
{
    public async Task<CuttingOrderDto?> Handle(GetCuttingOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var o = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricColor)
            .FirstOrDefaultAsync(o => o.Id == request.Id && !o.IsDeleted, cancellationToken);

        if (o is null) return null;

        var delivery = await context.CuttingDeliveries
            .FirstOrDefaultAsync(d => d.CuttingOrderId == o.Id, cancellationToken);

        return GetCuttingOrdersQueryHandler.ToDto(o, delivery?.GetDeliveredPieces());
    }
}
