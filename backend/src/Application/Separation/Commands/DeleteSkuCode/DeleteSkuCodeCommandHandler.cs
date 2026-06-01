using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Separation.Commands.DeleteSkuCode;

public class DeleteSkuCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteSkuCodeCommand>
{
    public async Task Handle(DeleteSkuCodeCommand request, CancellationToken cancellationToken)
    {
        var code = await context.SkuCodes
            .FirstOrDefaultAsync(c => c.Id == request.Id && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Código SKU não encontrado.");

        code.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
