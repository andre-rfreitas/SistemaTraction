using FluentValidation;

namespace SistemaTraction.Application.Config.Commands.UpsertAppConfig;

public class UpsertAppConfigCommandValidator : AbstractValidator<UpsertAppConfigCommand>
{
    public UpsertAppConfigCommandValidator()
    {
        RuleFor(x => x.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Value).NotNull().MaximumLength(1000);
    }
}
