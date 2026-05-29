using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricType;

public class CreateFabricTypeCommandValidator : AbstractValidator<CreateFabricTypeCommand>
{
    public CreateFabricTypeCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Variation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerKg).GreaterThan(0);
        RuleFor(x => x.AverageKgPerRoll).GreaterThan(0);
        RuleFor(x => x.AveragePiecesPerRoll).GreaterThan(0).When(x => x.AveragePiecesPerRoll.HasValue);
    }
}
