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
        context.SupplyTypes.Add(supplyType);

        var stockItem = SupplyStockItem.Create(supplyType.Id);
        context.SupplyStockItems.Add(stockItem);

        await context.SaveChangesAsync(cancellationToken);
        return supplyType.Id;
    }
}
