using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSeparationListById;

public class GetSeparationListByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSeparationListByIdQuery, SeparationListDetailDto?>
{
    public async Task<SeparationListDetailDto?> Handle(
        GetSeparationListByIdQuery request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.Id && !l.IsDeleted, cancellationToken);

        if (list is null) return null;

        var items = await context.SeparationItems
            .Where(i => i.SeparationListId == request.Id && !i.IsDeleted)
            .OrderBy(i => i.SortOrder)
            .ToListAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Estampa, i.Color, i.Size, i.Quantity, i.SortOrder, i.DtfModelId)).ToList()
        );
    }
}
