using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricType;

public class DeleteFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFabricTypeCommand>
{
    public async Task Handle(DeleteFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        fabricType.MarkAsDeleted();
        fabricType.TouchUpdatedAt();

        await context.SaveChangesAsync(cancellationToken);
    }
}
