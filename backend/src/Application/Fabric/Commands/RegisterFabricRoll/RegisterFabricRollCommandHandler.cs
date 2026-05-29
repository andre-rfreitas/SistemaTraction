using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Fabric.Commands.RegisterFabricRoll;

public class RegisterFabricRollCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterFabricRollCommand, RegisterFabricRollResult>
{
    public async Task<RegisterFabricRollResult> Handle(RegisterFabricRollCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == request.FabricTypeId && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        var color = fabricType.Colors.FirstOrDefault(c => c.Id == request.FabricColorId && !c.IsDeleted)
            ?? throw new DomainException("Cor não encontrada para este tipo de tecido.");

        var roll = FabricRoll.Create(request.FabricTypeId, request.FabricColorId, request.WeightKg, request.PriceTotal);
        context.FabricRolls.Add(roll);

        var financialAmount = request.WeightKg * fabricType.PricePerKg;
        var description = $"Bobina {color.Name} {fabricType.Name} {fabricType.Variation} — {request.WeightKg:F3}kg";
        var entry = FinancialEntry.CreateExpense("Tecido", financialAmount, description, roll.Id, "FabricRoll");
        context.FinancialEntries.Add(entry);

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterFabricRollResult(roll.Id, financialAmount, fabricType.PricePerKg);
    }
}
