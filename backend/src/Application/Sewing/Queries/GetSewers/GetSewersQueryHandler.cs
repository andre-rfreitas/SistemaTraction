using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Sewing.DTOs;

namespace SistemaTraction.Application.Sewing.Queries.GetSewers;

public class GetSewersQueryHandler(IApplicationDbContext db) : IRequestHandler<GetSewersQuery, List<SewerDto>>
{
    public async Task<List<SewerDto>> Handle(GetSewersQuery request, CancellationToken cancellationToken)
    {
        var query = db.Sewers
            .Include(s => s.ProductTypes)
            .AsNoTracking();

        if (!request.IncludeInactive)
            query = query.Where(s => s.IsActive);

        var sewers = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);

        return sewers.Select(s => new SewerDto(
            s.Id,
            s.Name,
            s.Phone,
            s.IsActive,
            s.ProductTypes
                .Where(pt => !pt.IsDeleted)
                .OrderBy(pt => pt.Name)
                .Select(pt => new SewerProductTypeDto(pt.Id, pt.Name, pt.PriceDefault, pt.PriceG1))
                .ToList()
        )).ToList();
    }
}
