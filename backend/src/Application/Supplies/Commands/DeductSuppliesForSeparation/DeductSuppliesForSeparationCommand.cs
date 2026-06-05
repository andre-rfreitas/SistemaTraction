using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.DeductSuppliesForSeparation;

public record DeductItem(Guid SupplyStockItemId, int Quantity);

public record DeductSuppliesForSeparationCommand(List<DeductItem> Items) : IRequest;
