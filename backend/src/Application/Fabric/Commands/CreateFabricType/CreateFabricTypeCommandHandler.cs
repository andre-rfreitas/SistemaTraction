using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public class CreateFabricTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFabricTypeCommand, Guid>
{
    public async Task<Guid> Handle(CreateFabricTypeCommand request, CancellationToken cancellationToken)
    {
        var fabricType = FabricType.Create(
            request.Name,
            request.Variation,
            request.PricePerKg,
            request.AverageKgPerRoll,
            request.AveragePiecesPerRoll);

        context.FabricTypes.Add(fabricType);
        await context.SaveChangesAsync(cancellationToken);

        return fabricType.Id;
    }
}
