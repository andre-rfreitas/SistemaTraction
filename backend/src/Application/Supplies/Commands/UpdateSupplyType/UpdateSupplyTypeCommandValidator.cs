using FluentValidation;

namespace SistemaTraction.Application.Supplies.Commands.UpdateSupplyType;

public class UpdateSupplyTypeCommandValidator : AbstractValidator<UpdateSupplyTypeCommand>
{
    public UpdateSupplyTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
