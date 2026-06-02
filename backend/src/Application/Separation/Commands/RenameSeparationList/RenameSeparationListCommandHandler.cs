using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Separation.Commands.RenameSeparationList;

public class RenameSeparationListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RenameSeparationListCommand, SeparationListSummaryDto>
{
    public async Task<SeparationListSummaryDto> Handle(
        RenameSeparationListCommand request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.Id && !l.IsDeleted, cancellationToken)
            ?? throw new DomainException("Lista de separação não encontrada.");

        list.Rename(request.FileName);

        var itemCount = await context.SeparationItems
            .CountAsync(i => i.SeparationListId == request.Id && !i.IsDeleted, cancellationToken);

        var totalQty = await context.SeparationItems
            .Where(i => i.SeparationListId == request.Id && !i.IsDeleted)
            .SumAsync(i => (int?)i.Quantity, cancellationToken) ?? 0;

        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListSummaryDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            itemCount, totalQty);
    }
}
