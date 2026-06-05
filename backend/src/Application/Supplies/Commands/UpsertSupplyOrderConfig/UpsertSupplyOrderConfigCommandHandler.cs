using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.UpsertSupplyOrderConfig;

public class UpsertSupplyOrderConfigCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpsertSupplyOrderConfigCommand>
{
    public async Task Handle(UpsertSupplyOrderConfigCommand request, CancellationToken cancellationToken)
    {
        var typeExists = await context.SupplyTypes
            .AnyAsync(t => t.Id == request.SupplyTypeId && !t.IsDeleted, cancellationToken);

        if (!typeExists)
            throw new DomainException("Tipo de insumo não encontrado.");

        var config = await context.SupplyOrderConfigs
            .FirstOrDefaultAsync(c => c.SupplyTypeId == request.SupplyTypeId, cancellationToken);

        if (config is null)
        {
            config = SupplyOrderConfig.Create(request.SupplyTypeId, request.QuantityPerOrder);
            context.SupplyOrderConfigs.Add(config);
        }
        else
        {
            config.Update(request.QuantityPerOrder);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
