using MediatR;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;

namespace SistemaTraction.Application.Sewing.Commands.RegisterPartialSewingDelivery;

public record RegisterPartialSewingDeliveryCommand(
    Guid OrderId,
    List<RegisterSewingDeliveryItemInput> Items
) : IRequest<RegisterSewingDeliveryResult>;
