using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Dtf.DTOs;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Queries.GetDtfOrders;

public class GetDtfOrdersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetDtfOrdersQuery, List<DtfOrderDto>>
{
    public async Task<List<DtfOrderDto>> Handle(GetDtfOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = context.DtfOrders
            .Include(o => o.Items)
            .Where(o => !o.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<DtfOrderStatus>(request.Status, out var statusFilter))
        {
            query = query.Where(o => o.Status == statusFilter);
        }

        var orders = await query
            .OrderByDescending(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        var modelIds = orders.SelectMany(o => o.Items)
            .Select(i => i.DtfModelId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var models = await context.DtfModels
            .Where(m => modelIds.Contains(m.Id) && !m.IsDeleted)
            .ToDictionaryAsync(m => m.Id, cancellationToken);

        return orders.Select(o => new DtfOrderDto(
            o.Id,
            o.OrderNumber,
            o.Status,
            o.Notes,
            o.SentAt,
            o.ReceivedAt,
            o.Items.Select(i =>
            {
                var model = i.DtfModelId.HasValue ? models.GetValueOrDefault(i.DtfModelId.Value) : null;
                return new DtfOrderItemDto(
                    i.Id,
                    i.DtfModelId,
                    model?.Name ?? "Modelo removido",
                    model?.SheetLabel ?? "-",
                    i.SheetQuantity,
                    model?.StampsPerSheet ?? 0,
                    i.SheetQuantity * (model?.StampsPerSheet ?? 0));
            }).ToList(),
            o.CreatedAt
        )).ToList();
    }
}
