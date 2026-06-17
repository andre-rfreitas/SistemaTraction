using MediatR;
using SistemaTraction.Application.Financial.DTOs;

namespace SistemaTraction.Application.Financial.Commands.UpdateOperationalExpense;

public record UpdateOperationalExpenseCommand(
    Guid Id,
    string Name,
    decimal FixedMonthlyValue,
    decimal RatePercent,
    bool IsActive
) : IRequest<OperationalExpenseDto>;
