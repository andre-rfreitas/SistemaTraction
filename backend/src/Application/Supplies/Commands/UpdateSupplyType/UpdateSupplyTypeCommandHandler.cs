using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;

public class UpdateSupplyTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSupplyTypeCommand>
{
    public async Task Handle(UpdateSupplyTypeCommand request, CancellationToken cancellationToken)
    {
        var supplyType = await context.SupplyTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de insumo não encontrado.");

        supplyType.Update(request.Name, request.Unit);
        await context.SaveChangesAsync(cancellationToken);
    }
}
