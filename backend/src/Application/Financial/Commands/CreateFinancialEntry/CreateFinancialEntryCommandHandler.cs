using MediatR;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Financial.Commands.CreateFinancialEntry;

public class CreateFinancialEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFinancialEntryCommand, CreateFinancialEntryResult>
{
    public async Task<CreateFinancialEntryResult> Handle(CreateFinancialEntryCommand request, CancellationToken cancellationToken)
    {
        var entry = request.Type switch
        {
            "Income" => FinancialEntry.CreateIncome(request.Category, request.Amount, request.Description),
            "Expense" => FinancialEntry.CreateExpense(request.Category, request.Amount, request.Description),
            _ => throw new DomainException("Tipo de lançamento inválido. Use 'Income' ou 'Expense'.")
        };

        context.FinancialEntries.Add(entry);
        await context.SaveChangesAsync(cancellationToken);

        return new CreateFinancialEntryResult(entry.Id);
    }
}
