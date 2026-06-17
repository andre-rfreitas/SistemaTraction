using MediatR;

namespace SistemaTraction.Application.Financial.Commands.DeleteOperationalExpense;

public record DeleteOperationalExpenseCommand(Guid Id) : IRequest;
