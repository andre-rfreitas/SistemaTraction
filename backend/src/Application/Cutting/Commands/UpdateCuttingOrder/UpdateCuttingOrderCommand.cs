using MediatR;

namespace SistemaTraction.Application.Cutting.Commands.UpdateCuttingOrder;

public record UpdateCuttingOrderItemInput(Guid FabricRollId, Dictionary<string, int> RequestedPieces);

public record UpdateCuttingOrderCommand(
    Guid OrderId,
    List<UpdateCuttingOrderItemInput> Items,
    string? Notes
) : IRequest<Unit>;
