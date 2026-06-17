using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetOperationalExpenses;

public class GetOperationalExpensesQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetOperationalExpensesQuery, List<OperationalExpenseDto>>
{
    public async Task<List<OperationalExpenseDto>> Handle(GetOperationalExpensesQuery request, CancellationToken cancellationToken)
    {
        return await context.OperationalExpenses
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Name)
            .Select(e => new OperationalExpenseDto(e.Id, e.Name, e.FixedMonthlyValue, e.RatePercent, e.IsActive))
            .ToListAsync(cancellationToken);
    }
}
