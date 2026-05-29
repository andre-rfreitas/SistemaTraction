using FluentValidation;

namespace SistemaTraction.Application.Fabric.Commands.UpdateFabricColor;

public class UpdateFabricColorCommandValidator : AbstractValidator<UpdateFabricColorCommand>
{
    public UpdateFabricColorCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.HexCode).Matches("^#[0-9A-Fa-f]{6}$").When(x => x.HexCode is not null && x.HexCode.Length > 0);
    }
}
