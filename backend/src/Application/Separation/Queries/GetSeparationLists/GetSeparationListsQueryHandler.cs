using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;

namespace SistemaTraction.Application.Separation.Queries.GetSeparationLists;

public class GetSeparationListsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSeparationListsQuery, List<SeparationListSummaryDto>>
{
    public async Task<List<SeparationListSummaryDto>> Handle(
        GetSeparationListsQuery request, CancellationToken cancellationToken)
    {
        var lists = await context.SeparationLists
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.UploadedAt)
            .ToListAsync(cancellationToken);

        var listIds = lists.Select(l => l.Id).ToList();

        var itemCounts = await context.SeparationItems
            .Where(i => listIds.Contains(i.SeparationListId) && !i.IsDeleted)
            .GroupBy(i => i.SeparationListId)
            .Select(g => new { g.Key, Count = g.Count(), Total = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(g => g.Key, g => (g.Count, g.Total), cancellationToken);

        return lists.Select(l =>
        {
            itemCounts.TryGetValue(l.Id, out var counts);
            return new SeparationListSummaryDto(
                l.Id, l.FileName, l.UploadedAt, l.Status.ToString(),
                counts.Count, counts.Total);
        }).ToList();
    }
}
