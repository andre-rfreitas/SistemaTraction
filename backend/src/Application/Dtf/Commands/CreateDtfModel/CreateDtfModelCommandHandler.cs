using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfModel;

public class CreateDtfModelCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateDtfModelCommand, Guid>
{
    public async Task<Guid> Handle(CreateDtfModelCommand request, CancellationToken cancellationToken)
    {
        var model = DtfModel.Create(
            request.Name,
            request.SheetLabel,
            request.StampsPerSheet,
            request.SheetCost);

        context.DtfModels.Add(model);
        await context.SaveChangesAsync(cancellationToken);

        return model.Id;
    }
}
