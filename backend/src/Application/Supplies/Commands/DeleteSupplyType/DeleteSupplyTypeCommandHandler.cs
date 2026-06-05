using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Supplies.Commands.DeleteSupplyType;

public class DeleteSupplyTypeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSupplyTypeCommand>
{
    public async Task Handle(DeleteSupplyTypeCommand request, CancellationToken cancellationToken)
    {
        var supplyType = await context.SupplyTypes
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de insumo não encontrado.");

        supplyType.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
