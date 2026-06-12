using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;

namespace SistemaTraction.Application.Sewing.Queries.GetSewerProductTypeNames;

public class GetSewerProductTypeNamesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSewerProductTypeNamesQuery, List<string>>
{
    public async Task<List<string>> Handle(GetSewerProductTypeNamesQuery request, CancellationToken cancellationToken)
    {
        return await context.SewerProductTypes
            .Where(pt => !pt.IsDeleted && !pt.Sewer!.IsDeleted)
            .Select(pt => pt.Name)
            .Distinct()
            .OrderBy(name => name)
            .ToListAsync(cancellationToken);
    }
}
