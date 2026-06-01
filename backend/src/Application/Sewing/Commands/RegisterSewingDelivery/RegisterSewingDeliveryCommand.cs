using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;

public record RegisterSewingDeliveryCommand(
    Guid OrderId,
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces
) : IRequest<RegisterSewingDeliveryResult>;
