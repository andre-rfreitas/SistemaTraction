using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricRoll;

public class DeleteFabricRollCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFabricRollCommand, Unit>
{
    public async Task<Unit> Handle(DeleteFabricRollCommand request, CancellationToken cancellationToken)
    {
        var roll = await context.FabricRolls
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new DomainException("Bobina não encontrada.");

        if (roll.Status == FabricRollStatus.InCutting)
            throw new DomainException("Bobina em corte não pode ser excluída. Cancele o pedido de corte primeiro.");

        roll.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
