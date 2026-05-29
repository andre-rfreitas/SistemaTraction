using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Config.DTOs;

namespace SistemaTraction.Application.Config.Queries.GetAppConfigByKey;

public class GetAppConfigByKeyQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAppConfigByKeyQuery, AppConfigDto?>
{
    public async Task<AppConfigDto?> Handle(
        GetAppConfigByKeyQuery request, CancellationToken cancellationToken)
    {
        return await context.AppConfigs
            .Where(c => c.Key == request.Key && !c.IsDeleted)
            .Select(c => new AppConfigDto(c.Key, c.Value, c.Description))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
