using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSkuCodes;

public class GetSkuCodesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSkuCodesQuery, List<SkuCodeDto>>
{
    public async Task<List<SkuCodeDto>> Handle(GetSkuCodesQuery request, CancellationToken cancellationToken)
    {
        return await context.SkuCodes
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.Category)
            .ThenBy(c => c.Code)
            .Select(c => new SkuCodeDto(c.Id, c.Code, c.Value, c.Category.ToString()))
            .ToListAsync(cancellationToken);
    }
}
