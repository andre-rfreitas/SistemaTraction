using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Sewing.Commands.DeleteSewer;

public class DeleteSewerCommandHandler(IApplicationDbContext db) : IRequestHandler<DeleteSewerCommand>
{
    public async Task Handle(DeleteSewerCommand request, CancellationToken cancellationToken)
    {
        var sewer = await db.Sewers
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new DomainException("Costureira não encontrada.");

        sewer.Deactivate();
        await db.SaveChangesAsync(cancellationToken);
    }
}
