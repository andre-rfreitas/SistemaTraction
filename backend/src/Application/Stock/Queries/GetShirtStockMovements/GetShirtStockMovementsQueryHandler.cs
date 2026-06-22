using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;


namespace SistemaTraction.Application.Stock.Queries.GetShirtStockMovements;

public class GetShirtStockMovementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetShirtStockMovementsQuery, ShirtStockMovementsDto>
{
    public async Task<ShirtStockMovementsDto> Handle(
        GetShirtStockMovementsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);

        var modelCode = string.IsNullOrWhiteSpace(request.ModelCode) ? "REG" : request.ModelCode.Trim().ToUpper();

        var query = context.ShirtStockMovements
            .Where(m => !m.IsDeleted && m.ModelCode == modelCode)
            .OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new ShirtStockMovementDto(
                m.Id,
                m.CreatedAt,
                m.FabricColorName,
                m.Size,
                m.ModelCode,
                m.Delta,
                m.Reason,
                m.Origin))
            .ToListAsync(cancellationToken);

        return new ShirtStockMovementsDto(items, totalCount, page, pageSize);
    }
}
