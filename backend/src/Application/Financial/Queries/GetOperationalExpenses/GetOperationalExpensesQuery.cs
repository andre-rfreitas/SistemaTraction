using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Queries.GetOperationalExpenses;

public record GetOperationalExpensesQuery : IRequest<List<OperationalExpenseDto>>;
