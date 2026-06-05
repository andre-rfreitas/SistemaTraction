using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public record CreateCuttingOrderItemInput(
    Guid FabricRollId,
    Dictionary<string, int> RequestedPieces
);

public record CreateCuttingOrderCommand(
    List<CreateCuttingOrderItemInput> Items,
    string? Notes,
    Dictionary<string, int>? RecommendedPieces = null,
    int? RecommendationDays = null,
    int? RecommendationBasedOnOrders = null
) : IRequest<CreateCuttingOrderResult>;
