using FluentValidation;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfOrder;

public class UpdateDtfOrderCommandValidator : AbstractValidator<UpdateDtfOrderCommand>
{
    public UpdateDtfOrderCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Notes).MaximumLength(500).When(c => c.Notes != null);
        RuleFor(c => c.Items).NotEmpty().WithMessage("O pedido deve ter pelo menos um modelo.");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.DtfModelId).NotEmpty();
            item.RuleFor(i => i.SheetQuantity).GreaterThan(0).WithMessage("Quantidade de folhas deve ser maior que zero.");
        });
    }
}
