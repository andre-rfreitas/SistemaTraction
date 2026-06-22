using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;


namespace SistemaTraction.Application.Stock.Queries.GetShirtStockItems;

public class GetShirtStockItemsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShirtStockItemsQuery, List<ShirtStockItemDto>>
{
    public async Task<List<ShirtStockItemDto>> Handle(
        GetShirtStockItemsQuery request, CancellationToken cancellationToken)
    {
        var modelCode = string.IsNullOrWhiteSpace(request.ModelCode) ? "REG" : request.ModelCode.Trim().ToUpper();

        var items = await context.StockItems
            .Where(s => !s.IsDeleted && s.ModelCode == modelCode)
            .OrderBy(s => s.FabricColorName)
            .ThenBy(s => s.Size)
            .Select(s => new ShirtStockItemDto(
                s.Id,
                s.FabricColorId,
                s.FabricColorName,
                s.Size,
                s.ModelCode,
                s.Quantity
            )).ToListAsync(cancellationToken);

        return items;
    }
}
