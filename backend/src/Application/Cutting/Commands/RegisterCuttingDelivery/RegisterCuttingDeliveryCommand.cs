using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;

public record RegisterCuttingDeliveryCommand(
    Guid CuttingOrderId,
    Dictionary<string, int> DeliveredPieces
) : IRequest<RegisterCuttingDeliveryResult>;
