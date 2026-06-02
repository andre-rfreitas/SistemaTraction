using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetFinancialEntries;

public class GetFinancialEntriesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetFinancialEntriesQuery, List<FinancialEntryDto>>
{
    public async Task<List<FinancialEntryDto>> Handle(GetFinancialEntriesQuery request, CancellationToken cancellationToken)
    {
        var query = context.FinancialEntries.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(e => e.Category == request.Category);

        if (request.From.HasValue)
            query = query.Where(e => e.EntryDate >= request.From.Value);

        if (request.To.HasValue)
            query = query.Where(e => e.EntryDate <= request.To.Value);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .ToListAsync(cancellationToken);

        var reversedIds = await context.FinancialEntries
            .Where(e => e.IsReversal && e.ReferenceId != null)
            .Select(e => e.ReferenceId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var reversedSet = reversedIds.ToHashSet();

        return entries.Select(e => new FinancialEntryDto(
            e.Id,
            e.Type.ToString(),
            e.Category,
            e.Amount,
            e.Description,
            e.ReferenceId,
            e.ReferenceType,
            e.EntryDate,
            e.IsReversal,
            reversedSet.Contains(e.Id),
            e.CreatedAt
        )).ToList();
    }
}
