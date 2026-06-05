using FluentValidation;

namespace SistemaTraction.Application.Supplies.Commands.CreateSupplyType;

public class CreateSupplyTypeCommandValidator : AbstractValidator<CreateSupplyTypeCommand>
{
    public CreateSupplyTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
    }
}
