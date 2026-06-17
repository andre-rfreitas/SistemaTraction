using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Commands.DeleteGenericProduct;

public record DeleteGenericProductCommand(Guid ProductId) : IRequest<Guid>;

public class DeleteGenericProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteGenericProductCommand, Guid>
{
    public async Task<Guid> Handle(DeleteGenericProductCommand request, CancellationToken cancellationToken)
    {
        var product = await context.GenericProducts
            .FirstOrDefaultAsync(p => p.Id == request.ProductId && !p.IsDeleted, cancellationToken)
            ?? throw new DomainException("Produto não encontrado.");

        if (product.Quantity > 0)
        {
            var movement = GenericProductMovement.Create(
                product.Id,
                product.Name,
                -product.Quantity,
                "Exclusão do produto",
                "Manual");
            context.GenericProductMovements.Add(movement);
        }

        product.MarkAsDeleted();
        product.TouchUpdatedAt();

        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
