using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Queries.GetShirtStockMovements;

public class GetShirtStockMovementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShirtStockMovementsQuery, ShirtStockMovementsDto>
{
    public async Task<ShirtStockMovementsDto> Handle(
        GetShirtStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var shirtType = Enum.TryParse<ShirtType>(request.ShirtType, out var parsed) ? parsed : ShirtType.Regular;

        var query = context.ShirtStockMovements
            .Where(m => !m.IsDeleted && m.ShirtType == shirtType)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ShirtStockMovementDto(
                m.Id,
                m.CreatedAt,
                m.FabricColorName,
                m.Size,
                m.ShirtType.ToString(),
                m.Delta,
                m.Reason,
                m.Origin))
            .ToListAsync(cancellationToken);

        return new ShirtStockMovementsDto(items, totalCount, page, pageSize);
    }
}
