using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfModels;

public class GetDtfModelsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfModelsQuery, List<DtfModelDto>>
{
    public async Task<List<DtfModelDto>> Handle(GetDtfModelsQuery request, CancellationToken cancellationToken)
    {
        return await context.DtfModels
            .Where(m => !m.IsDeleted)
            .OrderBy(m => m.SheetLabel)
            .Select(m => new DtfModelDto(m.Id, m.Name, m.SheetLabel, m.StampsPerSheet, m.SheetCost))
            .ToListAsync(cancellationToken);
    }
}
