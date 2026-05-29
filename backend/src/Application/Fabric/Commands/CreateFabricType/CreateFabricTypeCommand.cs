using MediatR;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public record CreateFabricTypeCommand(
    string Name,
    string Variation,
    decimal PricePerKg,
    decimal AverageKgPerRoll,
    int? AveragePiecesPerRoll
) : IRequest<Guid>;
