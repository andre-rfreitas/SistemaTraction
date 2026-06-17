using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Commands.CreateGenericProductCategory;

public class CreateGenericProductCategoryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateGenericProductCategoryCommand, Guid>
{
    public async Task<Guid> Handle(CreateGenericProductCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await context.GenericProductCategories.AnyAsync(c => c.Name == request.Name && !c.IsDeleted, cancellationToken))
            throw new DomainException("Já existe uma categoria com este nome.");

        var category = GenericProductCategory.Create(request.Name);
        context.GenericProductCategories.Add(category);
        await context.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
