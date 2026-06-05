using FluentValidation;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public class CreateCuttingOrderCommandValidator : AbstractValidator<CreateCuttingOrderCommand>
{
    public CreateCuttingOrderCommandValidator()
    {
        RuleFor(x => x.Items).NotEmpty().WithMessage("O pedido deve ter pelo menos uma bobina.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.FabricRollId).NotEmpty().WithMessage("Bobina é obrigatória.");
            item.RuleFor(i => i.RequestedPieces).NotEmpty().WithMessage("Informe as quantidades por tamanho.");
            item.RuleFor(i => i.RequestedPieces)
                .Must(p => p.Values.Sum() > 0)
                .WithMessage("Cada bobina deve ter pelo menos uma peça.");
            item.RuleFor(i => i.RequestedPieces)
                .Must(p => p.Values.All(v => v >= 0))
                .WithMessage("Quantidades não podem ser negativas.");
        });
    }
}
