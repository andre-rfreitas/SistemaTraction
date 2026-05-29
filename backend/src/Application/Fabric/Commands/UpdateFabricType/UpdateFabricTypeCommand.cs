using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public record UpdateFabricTypeCommand(
    Guid Id,
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll
) : IRequest;
