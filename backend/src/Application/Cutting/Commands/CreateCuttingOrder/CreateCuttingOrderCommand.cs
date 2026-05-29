using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public record CreateCuttingOrderCommand(
    Guid FabricRollId,
    Dictionary<string, int> RequestedPieces,
    string? Notes
) : IRequest<CreateCuttingOrderResult>;
