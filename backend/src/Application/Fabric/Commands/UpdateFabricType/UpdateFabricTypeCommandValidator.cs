using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricType;

public class UpdateFabricTypeCommandValidator : AbstractValidator<UpdateFabricTypeCommand>
{
    public UpdateFabricTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Variation).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PricePerKg).GreaterThan(0);
        RuleFor(x => x.AverageKgPerRoll).GreaterThan(0);
        RuleFor(x => x.AveragePiecesPerRoll).GreaterThan(0).When(x => x.AveragePiecesPerRoll.HasValue);
    }
}
