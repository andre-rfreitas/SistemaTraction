using FluentValidation;

namespace SistemaTraction.Application.Financial.Commands.CreateFinancialEntry;

public class CreateFinancialEntryCommandValidator : AbstractValidator<CreateFinancialEntryCommand>
{
    public CreateFinancialEntryCommandValidator()
    {
        RuleFor(x => x.Type)
            .Must(t => t is "Income" or "Expense")
            .WithMessage("Tipo deve ser 'Income' ou 'Expense'.");
        RuleFor(x => x.Category).NotEmpty().WithMessage("Categoria é obrigatória.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Valor deve ser maior que zero.");
        RuleFor(x => x.Description).NotEmpty().WithMessage("Descrição é obrigatória.");
    }
}
