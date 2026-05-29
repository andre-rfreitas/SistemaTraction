using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfStockItems;

public class GetDtfStockItemsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfStockItemsQuery, List<DtfStockItemDto>>
{
    public async Task<List<DtfStockItemDto>> Handle(
        GetDtfStockItemsQuery request, CancellationToken cancellationToken)
    {
        return await context.DtfStockItems
            .Where(i => !i.IsDeleted)
            .Select(i => new DtfStockItemDto(
                i.Id,
                i.DtfModelId,
                i.DtfModel.Name,
                i.DtfModel.SheetLabel,
                i.CurrentQuantity))
            .OrderBy(i => i.SheetLabel)
            .ToListAsync(cancellationToken);
    }
}
