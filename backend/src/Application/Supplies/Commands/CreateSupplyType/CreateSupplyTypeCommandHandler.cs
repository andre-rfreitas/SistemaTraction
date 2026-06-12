using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.CreateSupplyType;

public class CreateSupplyTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateSupplyTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateSupplyTypeCommand request, CancellationToken cancellationToken)
    {
        var supplyType = SupplyType.Create(request.Name, request.Unit, request.PricePerUnit);

        if (request.YieldBasis.HasValue && request.YieldBasis.Value != YieldBasis.None)
            supplyType.SetYield(request.YieldBasis.Value, request.YieldQuantity ?? 0, request.YieldProductName);

        context.SupplyTypes.Add(supplyType);

        var stockItem = SupplyStockItem.Create(supplyType.Id);
        context.SupplyStockItems.Add(stockItem);

        await context.SaveChangesAsync(cancellationToken);
        return supplyType.Id;
    }
}
