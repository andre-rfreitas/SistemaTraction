using FluentValidation;

namespace SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;

public class RegisterCuttingDeliveryCommandValidator : AbstractValidator<RegisterCuttingDeliveryCommand>
{
    public RegisterCuttingDeliveryCommandValidator()
    {
        RuleFor(x => x.CuttingOrderId).NotEmpty().WithMessage("Pedido é obrigatório.");
        RuleFor(x => x.DeliveredPieces).NotEmpty().WithMessage("Informe as peças entregues.");
        RuleFor(x => x.DeliveredPieces)
            .Must(p => p.Values.Sum() > 0)
            .WithMessage("A entrega deve ter pelo menos uma peça.");
        RuleFor(x => x.DeliveredPieces)
            .Must(p => p.Values.All(v => v >= 0))
            .WithMessage("Quantidades não podem ser negativas.");
    }
}
