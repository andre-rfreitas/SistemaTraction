using FluentValidation;

namespace SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;

public class RegisterCuttingDeliveryCommandValidator : AbstractValidator<RegisterCuttingDeliveryCommand>
{
    public RegisterCuttingDeliveryCommandValidator()
    {
        RuleFor(x => x.CuttingOrderId).NotEmpty().WithMessage("Pedido é obrigatório.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Informe as peças entregues.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.FabricRollId).NotEmpty().WithMessage("Bobina é obrigatória.");
            item.RuleFor(i => i.DeliveredPieces)
                .Must(p => p.Values.All(v => v >= 0))
                .WithMessage("Quantidades não podem ser negativas.");
        });
        RuleFor(x => x.Items)
            .Must(items => items.SelectMany(i => i.DeliveredPieces.Values).Sum() > 0)
            .WithMessage("A entrega deve ter pelo menos uma peça.");
    }
}
