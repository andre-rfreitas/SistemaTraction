using FluentValidation;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Application.Dtf.Commands.RegisterDtfMovement;

public class RegisterDtfMovementCommandValidator : AbstractValidator<RegisterDtfMovementCommand>
{
    public RegisterDtfMovementCommandValidator()
    {
        RuleFor(x => x.DtfModelId).NotEmpty();

        RuleFor(x => x.Type).IsInEnum();

        RuleFor(x => x.Quantity)
            .NotEqual(0).WithMessage("Quantidade não pode ser zero.")
            .Must((cmd, qty) => cmd.Type == DtfMovementType.Ajuste || qty > 0)
            .WithMessage("Quantidade deve ser maior que zero para Entrada e Saída.");

        RuleFor(x => x.Quantity)
            .LessThanOrEqualTo(1_000_000)
            .WithMessage("Quantidade máxima por movimento é 1.000.000.");

        RuleFor(x => x.Reason).MaximumLength(500);
    }
}
