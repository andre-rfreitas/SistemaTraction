using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyDeductionPreview;

public class GetSupplyDeductionPreviewQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplyDeductionPreviewQuery, List<SupplyDeductionPreviewItem>>
{
    public async Task<List<SupplyDeductionPreviewItem>> Handle(
        GetSupplyDeductionPreviewQuery request, CancellationToken cancellationToken)
    {
        var configs = await context.SupplyOrderConfigs
            .Where(c => !c.IsDeleted && c.QuantityPerOrder > 0)
            .Include(c => c.SupplyType)
            .ToListAsync(cancellationToken);

        var stockItems = await context.SupplyStockItems
            .Where(i => !i.IsDeleted)
            .ToListAsync(cancellationToken);

        return configs
            .Where(c => !c.SupplyType.IsDeleted)
            .OrderBy(c => c.SupplyType.Name)
            .Select(c =>
            {
                var stockItem = stockItems.FirstOrDefault(i => i.SupplyTypeId == c.SupplyTypeId);
                return new SupplyDeductionPreviewItem(
                    c.SupplyTypeId,
                    stockItem?.Id ?? Guid.Empty,
                    c.SupplyType.Name,
                    c.SupplyType.Unit,
                    c.QuantityPerOrder,
                    c.QuantityPerOrder * request.OrderCount);
            })
            .ToList();
    }
}
