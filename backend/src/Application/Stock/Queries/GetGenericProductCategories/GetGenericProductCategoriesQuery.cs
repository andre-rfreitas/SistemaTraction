using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Queries.GetGenericProductCategories;

public record GetGenericProductCategoriesQuery() : IRequest<List<GenericProductCategoryDto>>;

public class GetGenericProductCategoriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGenericProductCategoriesQuery, List<GenericProductCategoryDto>>
{
    public async Task<List<GenericProductCategoryDto>> Handle(GetGenericProductCategoriesQuery request, CancellationToken cancellationToken)
    {
        return await context.GenericProductCategories
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new GenericProductCategoryDto(c.Id, c.Name))
            .ToListAsync(cancellationToken);
    }
}
