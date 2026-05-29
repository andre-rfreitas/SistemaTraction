using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfStockItemByModel;

public class GetDtfStockItemByModelQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfStockItemByModelQuery, DtfStockItemDetailDto?>
{
    public async Task<DtfStockItemDetailDto?> Handle(
        GetDtfStockItemByModelQuery request, CancellationToken cancellationToken)
    {
        var item = await context.DtfStockItems
            .Where(i => i.DtfModelId == request.DtfModelId && !i.IsDeleted)
            .Select(i => new DtfStockItemDto(
                i.Id,
                i.DtfModelId,
                i.DtfModel.Name,
                i.DtfModel.SheetLabel,
                i.CurrentQuantity))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null) return null;

        var movements = await context.DtfStockMovements
            .Where(m => m.DtfStockItemId == item.Id && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Take(50)
            .Select(m => new DtfStockMovementDto(m.Id, m.Type, m.Delta, m.Reason, m.CreatedAt))
            .ToListAsync(cancellationToken);

        return new DtfStockItemDetailDto(item, movements);
    }
}
