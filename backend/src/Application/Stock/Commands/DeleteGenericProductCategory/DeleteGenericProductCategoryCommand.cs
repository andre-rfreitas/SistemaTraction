using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Stock.Commands.DeleteGenericProductCategory;

public record DeleteGenericProductCategoryCommand(Guid CategoryId) : IRequest<Guid>;

public class DeleteGenericProductCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteGenericProductCategoryCommand, Guid>
{
    public async Task<Guid> Handle(DeleteGenericProductCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.GenericProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Categoria não encontrada.");

        if (await context.GenericProducts.AnyAsync(p => p.CategoryId == request.CategoryId && !p.IsDeleted, cancellationToken))
            throw new DomainException("Não é possível excluir uma categoria que possui produtos.");

        category.MarkAsDeleted();
        category.TouchUpdatedAt();

        await context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
