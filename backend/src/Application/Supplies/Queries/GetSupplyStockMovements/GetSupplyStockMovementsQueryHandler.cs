using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyStockMovements;

public class GetSupplyStockMovementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplyStockMovementsQuery, List<SupplyStockMovementDto>>
{
    public async Task<List<SupplyStockMovementDto>> Handle(
        GetSupplyStockMovementsQuery request, CancellationToken cancellationToken)
    {
        return await context.SupplyStockMovements
            .Where(m => m.SupplyStockItemId == request.SupplyStockItemId)
            .OrderByDescending(m => m.OccurredAt)
            .Take(20)
            .Select(m => new SupplyStockMovementDto(
                m.Id,
                m.Type.ToString(),
                m.Delta,
                m.Reason,
                m.SupplierName,
                m.SupplierPhone,
                m.OccurredAt,
                m.UnitPrice,
                m.TotalCost,
                m.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
