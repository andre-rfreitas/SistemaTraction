using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.UpdateSeparationItems;

public class UpdateSeparationItemsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateSeparationItemsCommand, SeparationListDetailDto>
{
    public async Task<SeparationListDetailDto> Handle(
        UpdateSeparationItemsCommand request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.SeparationListId && !l.IsDeleted, cancellationToken)
            ?? throw new DomainException("Lista não encontrada.");

        if (list.Status != SeparationListStatus.Pending)
            throw new DomainException("Apenas listas pendentes podem ser editadas.");

        var items = await context.SeparationItems
            .Where(i => i.SeparationListId == request.SeparationListId && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var dto in request.Items)
        {
            var item = items.FirstOrDefault(i => i.Id == dto.Id)
                ?? throw new DomainException($"Item {dto.Id} não encontrado.");

            item.Update(dto.Sku, dto.Color, dto.Size, dto.Quantity);
            item.SetDtfModel(dto.DtfModelId);
        }

        await context.SaveChangesAsync(cancellationToken);

        // Load DTF model names for response
        var dtfModelIds = items.Where(i => i.DtfModelId.HasValue).Select(i => i.DtfModelId!.Value).Distinct().ToList();
        var dtfModels = await context.DtfModels
            .Where(m => dtfModelIds.Contains(m.Id))
            .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);

        return new SeparationListDetailDto(
            list.Id,
            list.FileName,
            list.UploadedAt,
            list.Status.ToString(),
            items.OrderBy(i => i.SortOrder)
                 .Select(i => new SeparationItemDto(
                     i.Id, i.Sku, i.Color, i.Size, i.Quantity,
                     i.DtfModelId,
                     i.DtfModelId.HasValue ? dtfModels.GetValueOrDefault(i.DtfModelId.Value) : null,
                     i.SortOrder))
                 .ToList()
        );
    }
}
