using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyStockItems;

public class GetSupplyStockItemsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplyStockItemsQuery, List<SupplyStockItemDto>>
{
    public async Task<List<SupplyStockItemDto>> Handle(
        GetSupplyStockItemsQuery request, CancellationToken cancellationToken)
    {
        return await context.SupplyStockItems
            .Where(i => !i.IsDeleted && !i.SupplyType.IsDeleted)
            .OrderBy(i => i.SupplyType.Name)
            .Select(i => new SupplyStockItemDto(
                i.Id,
                i.SupplyTypeId,
                i.SupplyType.Name,
                i.SupplyType.Unit,
                i.Quantity))
            .ToListAsync(cancellationToken);
    }
}
