using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public class UpdateFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFabricTypeCommand>
{
    public async Task Handle(UpdateFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        fabricType.Update(request.Name, request.Variation, request.PricePerKg, request.AverageKgPerRoll, request.AveragePiecesPerRoll);

        await context.SaveChangesAsync(cancellationToken);
    }
}
