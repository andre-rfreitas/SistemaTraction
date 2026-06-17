using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStockItems;

public class GetShirtStockItemsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShirtStockItemsQuery, List<ShirtStockItemDto>>
{
    public async Task<List<ShirtStockItemDto>> Handle(
        GetShirtStockItemsQuery request, CancellationToken cancellationToken)
    {
        var shirtType = Enum.TryParse<ShirtType>(request.ShirtType, out var parsed) ? parsed : ShirtType.Regular;

        var items = await context.StockItems
            .Where(s => !s.IsDeleted && s.ShirtType == shirtType)
            .OrderBy(s => s.FabricColorName)
            .ThenBy(s => s.Size)
            .ToListAsync(cancellationToken);

        return items.Select(s => new ShirtStockItemDto(
            s.Id,
            s.FabricColorId,
            s.FabricColorName,
            s.Size,
            s.ShirtType.ToString(),
            s.Quantity
        )).ToList();
    }
}
