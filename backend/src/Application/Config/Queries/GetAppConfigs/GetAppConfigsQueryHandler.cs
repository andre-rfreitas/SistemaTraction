using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Config.DTOs;

namespace SistemaTraction.Application.Config.Queries.GetAppConfigs;

public class GetAppConfigsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAppConfigsQuery, List<AppConfigDto>>
{
    public async Task<List<AppConfigDto>> Handle(
        GetAppConfigsQuery request, CancellationToken cancellationToken)
    {
        return await context.AppConfigs
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Key)
            .Select(c => new AppConfigDto(c.Key, c.Value, c.Description))
            .ToListAsync(cancellationToken);
    }
}
