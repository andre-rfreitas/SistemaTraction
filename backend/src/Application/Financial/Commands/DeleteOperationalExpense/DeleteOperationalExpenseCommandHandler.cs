using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Financial.Commands.DeleteOperationalExpense;

public class DeleteOperationalExpenseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteOperationalExpenseCommand>
{
    public async Task Handle(DeleteOperationalExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await context.OperationalExpenses
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken)
            ?? throw new DomainException("Despesa operacional não encontrada.");

        expense.MarkAsDeleted();
        await context.SaveChangesAsync(cancellationToken);
    }
}
