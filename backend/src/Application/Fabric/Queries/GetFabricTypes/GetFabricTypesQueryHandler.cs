using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricTypes;

public class GetFabricTypesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricTypesQuery, List<FabricTypeDto>>
{
    public async Task<List<FabricTypeDto>> Handle(GetFabricTypesQuery request, CancellationToken cancellationToken)
    {
        var types = await context.FabricTypes
            .Include(t => t.Colors.Where(c => !c.IsDeleted))
            .Where(t => !t.IsDeleted)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return types.Select(t => new FabricTypeDto(
            t.Id,
            t.Name,
            t.Variation,
            t.PricePerKg,
            t.AverageKgPerRoll,
            t.AveragePiecesPerRoll,
            t.Colors.Select(c => new FabricColorDto(c.Id, c.FabricTypeId, c.Name, c.HexCode)).ToList()
        )).ToList();
    }
}
