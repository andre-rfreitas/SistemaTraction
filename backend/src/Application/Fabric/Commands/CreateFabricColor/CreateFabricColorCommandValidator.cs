using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public class CreateFabricColorCommandValidator : AbstractValidator<CreateFabricColorCommand>
{
    public CreateFabricColorCommandValidator()
    {
        RuleFor(x => x.FabricTypeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HexCode).Matches("^#[0-9A-Fa-f]{6}$").When(x => x.HexCode is not null && x.HexCode.Length > 0);
    }
}
