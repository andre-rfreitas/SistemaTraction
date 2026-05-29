using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypeById;

public class GetFabricTypeByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricTypeByIdQuery, FabricTypeDto?>
{
    public async Task<FabricTypeDto?> Handle(GetFabricTypeByIdQuery request, CancellationToken cancellationToken)
    {
        var t = await context.FabricTypes
            .Include(t => t.Colors.Where(c => !c.IsDeleted))
            .Where(t => t.Id == request.Id && !t.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (t is null) return null;

        return new FabricTypeDto(
            t.Id,
            t.Name,
            t.Variation,
            t.PricePerKg,
            t.AverageKgPerRoll,
            t.AveragePiecesPerRoll,
            t.Colors.Select(c => new FabricColorDto(c.Id, c.FabricTypeId, c.Name, c.HexCode)).ToList()
        );
    }
}
