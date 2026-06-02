using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Separation.Commands.DeleteSeparationList;

public class DeleteSeparationListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSeparationListCommand>
{
    public async Task Handle(DeleteSeparationListCommand request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.Id && !l.IsDeleted, cancellationToken)
            ?? throw new DomainException("Lista de separação não encontrada.");

        if (list.Status.ToString() == "Confirmed")
            throw new DomainException("Listas confirmadas não podem ser excluídas.");

        // Also soft-delete associated items
        var items = await context.SeparationItems
            .Where(i => i.SeparationListId == request.Id && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            item.MarkAsDeleted();

        list.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
