using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfModel;

public class UpdateDtfModelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateDtfModelCommand>
{
    public async Task Handle(UpdateDtfModelCommand request, CancellationToken cancellationToken)
    {
        var model = await context.DtfModels
            .FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, cancellationToken)
            ?? throw new DomainException("Modelo DTF não encontrado.");

        model.Update(request.Name, request.SheetLabel, request.StampsPerSheet, request.SheetCost);
        await context.SaveChangesAsync(cancellationToken);
    }
}
