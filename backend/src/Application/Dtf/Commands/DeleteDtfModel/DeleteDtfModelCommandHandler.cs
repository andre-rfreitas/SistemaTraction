using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.DeleteDtfModel;

public class DeleteDtfModelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteDtfModelCommand>
{
    public async Task Handle(DeleteDtfModelCommand request, CancellationToken cancellationToken)
    {
        var model = await context.DtfModels
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken)
            ?? throw new DomainException("Modelo DTF não encontrado.");

        model.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
