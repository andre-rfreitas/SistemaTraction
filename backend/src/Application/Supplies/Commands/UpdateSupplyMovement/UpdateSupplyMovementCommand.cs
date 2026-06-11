using MediatR;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyMovement;

public record UpdateSupplyMovementCommand(
    Guid MovementId,
    string? SupplierName,
    string? SupplierPhone,
    DateTime? OccurredAt,
    decimal? UnitPrice,
    decimal? TotalCost) : IRequest;
