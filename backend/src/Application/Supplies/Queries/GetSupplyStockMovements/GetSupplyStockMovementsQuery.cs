using MediatR;
using SistemaTraction.Application.Supplies.DTOs;

namespace SistemaTraction.Application.Supplies.Queries.GetSupplyStockMovements;

public record GetSupplyStockMovementsQuery(Guid SupplyStockItemId) : IRequest<List<SupplyStockMovementDto>>;
