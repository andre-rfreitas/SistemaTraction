using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.RegisterFabricRoll;

public class RegisterFabricRollCommandValidator : AbstractValidator<RegisterFabricRollCommand>
{
    public RegisterFabricRollCommandValidator()
    {
        RuleFor(x => x.FabricTypeId).NotEmpty().WithMessage("Tipo de tecido é obrigatório.");
        RuleFor(x => x.FabricColorId).NotEmpty().WithMessage("Cor é obrigatória.");
        RuleFor(x => x.WeightKg).GreaterThan(0).WithMessage("Peso deve ser maior que zero.");
        RuleFor(x => x.PriceTotal).GreaterThan(0).WithMessage("Preço total deve ser maior que zero.");
    }
}
