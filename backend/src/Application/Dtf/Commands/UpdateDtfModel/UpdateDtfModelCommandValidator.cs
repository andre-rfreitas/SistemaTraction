using FluentValidation;

namespace SistemaTraction.Application.Dtf.Commands.UpdateDtfModel;

public class UpdateDtfModelCommandValidator : AbstractValidator<UpdateDtfModelCommand>
{
    public UpdateDtfModelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.SheetLabel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.StampsPerSheet).GreaterThan(0);
        RuleFor(x => x.SheetCost).GreaterThan(0);
    }
}
