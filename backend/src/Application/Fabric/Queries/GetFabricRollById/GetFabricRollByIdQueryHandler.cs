using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Fabric.DTOs;

namespace SistemaTraction.Application.Fabric.Queries.GetFabricRollById;

public class GetFabricRollByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFabricRollByIdQuery, FabricRollDto?>
{
    public async Task<FabricRollDto?> Handle(GetFabricRollByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await context.FabricRolls
            .Include(r => r.FabricType)
            .Include(r => r.FabricColor)
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (r is null) return null;

        return new FabricRollDto(
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
        );
    }
}
