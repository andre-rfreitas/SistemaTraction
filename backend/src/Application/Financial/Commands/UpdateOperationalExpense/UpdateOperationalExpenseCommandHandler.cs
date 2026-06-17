using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Financial.Commands.UpdateOperationalExpense;

public class UpdateOperationalExpenseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpdateOperationalExpenseCommand, OperationalExpenseDto>
{
    public async Task<OperationalExpenseDto> Handle(UpdateOperationalExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await context.OperationalExpenses
            .FirstOrDefaultAsync(e => e.Id == request.Id && !e.IsDeleted, cancellationToken)
            ?? throw new DomainException("Despesa operacional não encontrada.");

        expense.Update(request.Name, request.FixedMonthlyValue, request.RatePercent, request.IsActive);
        await context.SaveChangesAsync(cancellationToken);
        return new OperationalExpenseDto(expense.Id, expense.Name, expense.FixedMonthlyValue, expense.RatePercent, expense.IsActive);
    }
}
