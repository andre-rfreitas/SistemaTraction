using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetGenericProducts;

public record GetGenericProductsQuery(Guid CategoryId) : IRequest<List<GenericProductDto>>;

public class GetGenericProductsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGenericProductsQuery, List<GenericProductDto>>
{
    public async Task<List<GenericProductDto>> Handle(GetGenericProductsQuery request, CancellationToken cancellationToken)
    {
        return await context.GenericProducts
            .Where(p => p.CategoryId == request.CategoryId && !p.IsDeleted)
            .OrderBy(p => p.Name)
            .Select(p => new GenericProductDto(p.Id, p.CategoryId, p.Name, p.Quantity))
            .ToListAsync(cancellationToken);
    }
}
