using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetGenericProductMovements;

public record GetGenericProductMovementsQuery(Guid ProductId, int Page = 1, int PageSize = 20) : IRequest<GenericProductMovementsResponseDto>;

public class GetGenericProductMovementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGenericProductMovementsQuery, GenericProductMovementsResponseDto>
{
    public async Task<GenericProductMovementsResponseDto> Handle(GetGenericProductMovementsQuery request, CancellationToken cancellationToken)
    {
        var query = context.GenericProductMovements
            .Where(m => m.GenericProductId == request.ProductId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(m => new GenericProductMovementDto(
                m.Id,
                m.CreatedAt,
                m.ProductName,
                m.Delta,
                m.Reason,
                m.Origin
            ))
            .ToListAsync(cancellationToken);

        return new GenericProductMovementsResponseDto(items, total, request.Page, request.PageSize);
    }
}
