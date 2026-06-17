using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Stock.Commands.CreateGenericProduct;

public class CreateGenericProductCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateGenericProductCommand, Guid>
{
    public async Task<Guid> Handle(CreateGenericProductCommand request, CancellationToken cancellationToken)
    {
        var category = await context.GenericProductCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && !c.IsDeleted, cancellationToken)
            ?? throw new DomainException("Categoria não encontrada.");

        if (await context.GenericProducts.AnyAsync(p => p.CategoryId == request.CategoryId && p.Name == request.Name && !p.IsDeleted, cancellationToken))
            throw new DomainException("Já existe um produto com este nome nesta categoria.");

        var product = GenericProduct.Create(request.CategoryId, request.Name, request.InitialQuantity);
        context.GenericProducts.Add(product);

        if (request.InitialQuantity > 0)
        {
            var movement = GenericProductMovement.Create(
                product.Id,
                product.Name,
                request.InitialQuantity,
                request.Reason,
                "Manual");
            context.GenericProductMovements.Add(movement);

            if (request.UnitCost > 0)
            {
                var totalCost = request.UnitCost * request.InitialQuantity;
                var financialEntry = FinancialEntry.CreateExpense(
                    FinancialCategories.Estoque,
                    totalCost,
                    $"Entrada de estoque: {category.Name} - {product.Name} ({request.InitialQuantity} un. × R$ {request.UnitCost:N2})",
                    referenceId: movement.Id,
                    referenceType: "GenericProductMovement");
                context.FinancialEntries.Add(financialEntry);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
