using FluentValidation;

namespace SistemaTraction.Application.Dtf.Commands.CreateDtfModel;

public class CreateDtfModelCommandValidator : AbstractValidator<CreateDtfModelCommand>
{
    public CreateDtfModelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SheetLabel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StampsPerSheet).GreaterThan(0);
        RuleFor(x => x.SheetCost).GreaterThan(0);
    }
}
