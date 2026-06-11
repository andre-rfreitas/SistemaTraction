using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Sewing.DTOs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Sewing.Commands.UpdateSewer;

public class UpdateSewerCommandHandler(IApplicationDbContext db) : IRequestHandler<UpdateSewerCommand, SewerDto>
{
    public async Task<SewerDto> Handle(UpdateSewerCommand request, CancellationToken cancellationToken)
    {
        var sewer = await db.Sewers
            .Include(s => s.ProductTypes)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new DomainException("Costureira não encontrada.");

        sewer.Update(request.Name, request.Phone);

        var existing = sewer.ProductTypes.Where(pt => !pt.IsDeleted).ToList();
        var toKeep = request.ProductTypes
            .Where(pt => existing.Any(e => e.Name.Equals(pt.Name, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        foreach (var e in existing)
        {
            var match = request.ProductTypes.FirstOrDefault(pt =>
                pt.Name.Equals(e.Name, StringComparison.OrdinalIgnoreCase));

            if (match is null)
                sewer.RemoveProductType(e.Id);
            else
                sewer.UpdateProductType(e.Id, match.Name, match.PriceDefault, match.PriceG1);
        }

        foreach (var pt in request.ProductTypes)
        {
            if (!existing.Any(e => e.Name.Equals(pt.Name, StringComparison.OrdinalIgnoreCase)))
                sewer.AddProductType(pt.Name, pt.PriceDefault, pt.PriceG1);
        }

        await db.SaveChangesAsync(cancellationToken);

        return new SewerDto(
            sewer.Id,
            sewer.Name,
            sewer.Phone,
            sewer.IsActive,
            sewer.ProductTypes
                .Where(pt => !pt.IsDeleted)
                .OrderBy(pt => pt.Name)
                .Select(pt => new SewerProductTypeDto(pt.Id, pt.Name, pt.PriceDefault, pt.PriceG1))
                .ToList()
        );
    }
}
