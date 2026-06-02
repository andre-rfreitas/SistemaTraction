using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public record CreateCuttingOrderCommand(
    Guid FabricRollId,
    Dictionary<string, int> RequestedPieces,
    string? Notes,
    Dictionary<string, int>? RecommendedPieces = null,
    int? RecommendationDays = null,
    int? RecommendationBasedOnOrders = null
) : IRequest<CreateCuttingOrderResult>;
