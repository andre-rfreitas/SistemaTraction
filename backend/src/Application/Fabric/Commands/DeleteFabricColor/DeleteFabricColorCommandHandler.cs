using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.DeleteFabricColor;

public class DeleteFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteFabricColorCommand>
{
    public async Task Handle(DeleteFabricColorCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == request.FabricTypeId && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        fabricType.RemoveColor(request.ColorId);

        await context.SaveChangesAsync(cancellationToken);
    }
}
