using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Financial.Commands.CreateOperationalExpense;

public class CreateOperationalExpenseCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateOperationalExpenseCommand, OperationalExpenseDto>
{
    public async Task<OperationalExpenseDto> Handle(CreateOperationalExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = OperationalExpense.Create(request.Name, request.FixedMonthlyValue, request.RatePercent);
        context.OperationalExpenses.Add(expense);
        await context.SaveChangesAsync(cancellationToken);
        return new OperationalExpenseDto(expense.Id, expense.Name, expense.FixedMonthlyValue, expense.RatePercent, expense.IsActive);
    }
}
