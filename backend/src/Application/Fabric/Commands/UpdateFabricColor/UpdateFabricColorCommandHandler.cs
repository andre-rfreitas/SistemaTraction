using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public class UpdateFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFabricColorCommand>
{
    public async Task Handle(UpdateFabricColorCommand request, CancellationToken cancellationToken)
    {
        var color = await context.FabricColors
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Cor não encontrada.");

        color.Update(request.Name, request.HexCode);

        await context.SaveChangesAsync(cancellationToken);
    }
}
