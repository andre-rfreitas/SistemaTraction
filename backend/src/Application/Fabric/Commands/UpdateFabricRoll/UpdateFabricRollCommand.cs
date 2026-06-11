using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricRoll;

public record UpdateFabricRollCommand(
    Guid Id,
    Guid FabricTypeId,
    Guid FabricColorId,
    decimal WeightKg,
    decimal PriceTotal
) : IRequest;
