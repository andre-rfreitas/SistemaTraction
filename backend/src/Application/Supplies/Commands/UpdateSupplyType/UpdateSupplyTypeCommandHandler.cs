using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;

public class UpdateSupplyTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSupplyTypeCommand>
{
    public async Task Handle(UpdateSupplyTypeCommand request, CancellationToken cancellationToken)
    {
        var supplyType = await context.SupplyTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de insumo não encontrado.");

        supplyType.Update(request.Name, request.Unit, request.PricePerUnit);

        if (request.YieldBasis.HasValue && request.YieldBasis.Value != YieldBasis.None)
            supplyType.SetYield(request.YieldBasis.Value, request.YieldQuantity ?? 0, request.YieldProductName);
        else
            supplyType.ClearYield();

        await context.SaveChangesAsync(cancellationToken);
    }
}
