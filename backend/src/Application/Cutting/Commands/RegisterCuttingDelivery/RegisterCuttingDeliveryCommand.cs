using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;

public record RegisterCuttingDeliveryItemInput(
    Guid FabricRollId,
    Dictionary<string, int> DeliveredPieces
);

public record RegisterCuttingDeliveryCommand(
    Guid CuttingOrderId,
    List<RegisterCuttingDeliveryItemInput> Items
) : IRequest<RegisterCuttingDeliveryResult>;
