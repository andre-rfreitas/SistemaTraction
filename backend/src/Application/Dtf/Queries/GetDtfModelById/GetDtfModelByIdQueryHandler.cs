using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfModelById;

public class GetDtfModelByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfModelByIdQuery, DtfModelDto?>
{
    public async Task<DtfModelDto?> Handle(GetDtfModelByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.DtfModels
            .Where(m => m.Id == request.Id && !m.IsDeleted)
            .Select(m => new DtfModelDto(m.Id, m.Name, m.SheetLabel, m.StampsPerSheet, m.SheetCost))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
