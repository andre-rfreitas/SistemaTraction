using FluentValidation;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public class CreateCuttingOrderCommandValidator : AbstractValidator<CreateCuttingOrderCommand>
{
    public CreateCuttingOrderCommandValidator()
    {
        RuleFor(x => x.FabricRollId).NotEmpty().WithMessage("Bobina é obrigatória.");
        RuleFor(x => x.RequestedPieces).NotEmpty().WithMessage("Informe as quantidades por tamanho.");
        RuleFor(x => x.RequestedPieces)
            .Must(p => p.Values.Sum() > 0)
            .WithMessage("O pedido deve ter pelo menos uma peça.");
        RuleFor(x => x.RequestedPieces)
            .Must(p => p.Values.All(v => v >= 0))
            .WithMessage("Quantidades não podem ser negativas.");
    }
}
