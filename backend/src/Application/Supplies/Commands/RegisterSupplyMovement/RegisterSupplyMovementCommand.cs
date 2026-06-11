using MediatR;
using SistemaTraction.Application.Supplies.DTOs;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Supplies.Commands.RegisterSupplyMovement;

public record RegisterSupplyMovementCommand(
    Guid SupplyStockItemId,
    SupplyMovementType Type,
    int Quantity,
    string? Reason,
    string? SupplierName = null,
    string? SupplierPhone = null,
    DateTime? OccurredAt = null,
    decimal? UnitPrice = null,
    decimal? TotalCost = null) : IRequest<RegisterSupplyMovementResult>;
