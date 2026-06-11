using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricRoll;

public class UpdateFabricRollCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateFabricRollCommand>
{
    public async Task Handle(UpdateFabricRollCommand request, CancellationToken cancellationToken)
    {
        var roll = await context.FabricRolls
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken)
            ?? throw new DomainException("Bobina não encontrada.");

        var fabricType = await context.FabricTypes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == request.FabricTypeId && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        var color = fabricType.Colors.FirstOrDefault(c => c.Id == request.FabricColorId && !c.IsDeleted)
            ?? throw new DomainException("Cor não encontrada para este tipo de tecido.");

        roll.Update(request.FabricTypeId, request.FabricColorId, request.WeightKg, request.PriceTotal);

        var financialEntry = await context.FinancialEntries
            .FirstOrDefaultAsync(e => e.ReferenceId == roll.Id && e.ReferenceType == "FabricRoll" && !e.IsReversal, cancellationToken);

        if (financialEntry is not null)
        {
            var financialAmount = request.WeightKg * fabricType.PricePerKg;
            var description = $"Bobina {color.Name} {fabricType.Name} {fabricType.Variation} — {request.WeightKg:F3}kg";
            financialEntry.UpdateAmount(financialAmount, description);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
