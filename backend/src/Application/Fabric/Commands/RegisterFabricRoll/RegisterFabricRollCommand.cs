using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.RegisterFabricRoll;

public record RegisterFabricRollCommand(
    Guid FabricTypeId,
    Guid FabricColorId,
    decimal WeightKg,
    decimal PriceTotal
) : IRequest<RegisterFabricRollResult>;

public record RegisterFabricRollResult(
    Guid FabricRollId,
    decimal FinancialEntryAmount,
    decimal FabricTypePricePerKg
);
