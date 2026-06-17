using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Commands.CreateOperationalExpense;

public record CreateOperationalExpenseCommand(
    string Name,
    decimal FixedMonthlyValue,
    decimal RatePercent
) : IRequest<OperationalExpenseDto>;
