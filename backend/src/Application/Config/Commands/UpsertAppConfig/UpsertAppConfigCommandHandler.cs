using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Config;

namespace SistemaTraction.Application.Config.Commands.UpsertAppConfig;

public class UpsertAppConfigCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpsertAppConfigCommand>
{
    public async Task Handle(UpsertAppConfigCommand request, CancellationToken cancellationToken)
    {
        var config = await context.AppConfigs
            .FirstOrDefaultAsync(c => c.Key == request.Key && !c.IsDeleted, cancellationToken);

        if (config is null)
        {
            config = AppConfig.Create(request.Key, request.Value);
            context.AppConfigs.Add(config);
        }
        else
        {
            config.UpdateValue(request.Value);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
