using MediatR;
using SistemaTraction.Application.Cutting.DTOs;

namespace SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;

public record RegisterSewingDeliveryItemInput(
    Guid FabricRollId,
    Dictionary<string, int> GoodPieces,
    Dictionary<string, int> DefectivePieces
);

public record RegisterSewingDeliveryCommand(
    Guid OrderId,
    List<RegisterSewingDeliveryItemInput> Items
) : IRequest<RegisterSewingDeliveryResult>;
