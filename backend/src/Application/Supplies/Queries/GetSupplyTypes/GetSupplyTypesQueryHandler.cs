using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyTypes;

public class GetSupplyTypesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplyTypesQuery, List<SupplyTypeDto>>
{
    public async Task<List<SupplyTypeDto>> Handle(GetSupplyTypesQuery request, CancellationToken cancellationToken)
    {
        return await context.SupplyTypes
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .Select(t => new SupplyTypeDto(t.Id, t.Name, t.Unit))
            .ToListAsync(cancellationToken);
    }
}
