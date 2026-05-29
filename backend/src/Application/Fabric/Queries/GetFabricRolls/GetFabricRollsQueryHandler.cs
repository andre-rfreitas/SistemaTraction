using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricRolls;

public class GetFabricRollsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricRollsQuery, List<FabricRollDto>>
{
    public async Task<List<FabricRollDto>> Handle(GetFabricRollsQuery request, CancellationToken cancellationToken)
    {
        var query = context.FabricRolls
            .Include(r => r.FabricType)
            .Include(r => r.FabricColor)
            .Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<FabricRollStatus>(request.Status, ignoreCase: true, out var status))
        {
            query = query.Where(r => r.Status == status);
        }

        var rolls = await query
            .OrderByDescending(r => r.ReceivedAt)
            .ToListAsync(cancellationToken);

        return rolls.Select(r => new FabricRollDto(
            r.Id,
            r.FabricTypeId,
            r.FabricType!.Name,
            r.FabricType!.Variation,
            r.FabricType!.PricePerKg,
            r.FabricColorId,
            r.FabricColor!.Name,
            r.FabricColor!.HexCode,
            r.WeightKg,
            r.PriceTotal,
            r.PricePerKgActual,
            r.ReceivedAt,
            r.Status.ToString(),
            r.CreatedAt
        )).ToList();
    }
}
