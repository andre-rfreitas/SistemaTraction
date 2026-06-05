using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyOrderConfigs;

public class GetSupplyOrderConfigsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSupplyOrderConfigsQuery, List<SupplyOrderConfigDto>>
{
    public async Task<List<SupplyOrderConfigDto>> Handle(
        GetSupplyOrderConfigsQuery request, CancellationToken cancellationToken)
    {
        var types = await context.SupplyTypes
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        var configs = await context.SupplyOrderConfigs
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        return types.Select(t =>
        {
            var config = configs.FirstOrDefault(c => c.SupplyTypeId == t.Id);
            return new SupplyOrderConfigDto(t.Id, t.Name, t.Unit, config?.QuantityPerOrder ?? 0);
        }).ToList();
    }
}
