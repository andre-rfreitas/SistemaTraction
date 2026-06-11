using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Sewing.DTOs;
using SistemaTraction.Domain.Sewing;

namespace SistemaTraction.Application.Sewing.Commands.CreateSewer;

public class CreateSewerCommandHandler(IApplicationDbContext db) : IRequestHandler<CreateSewerCommand, SewerDto>
{
    public async Task<SewerDto> Handle(CreateSewerCommand request, CancellationToken cancellationToken)
    {
        var sewer = Sewer.Create(request.Name, request.Phone);

        foreach (var pt in request.ProductTypes)
            sewer.AddProductType(pt.Name, pt.PriceDefault, pt.PriceG1);

        db.Sewers.Add(sewer);
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
