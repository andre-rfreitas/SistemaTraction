using FluentValidation;

namespace SistemaTraction.Application.Stock.Commands.AdjustShirtStock;

public class AdjustShirtStockCommandValidator : AbstractValidator<AdjustShirtStockCommand>
{
    public AdjustShirtStockCommandValidator()
    {
        RuleFor(x => x.FabricColorId).NotEmpty().WithMessage("Cor é obrigatória.");
        RuleFor(x => x.Size).NotEmpty().WithMessage("Tamanho é obrigatório.");
        RuleFor(x => x.AdjustmentType)
            .Must(t => t == "Entrada" || t == "Saída")
            .WithMessage("Tipo deve ser 'Entrada' ou 'Saída'.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Motivo é obrigatório.");
        RuleFor(x => x.ShirtType)
            .Must(t => t == "Regular" || t == "Over")
            .WithMessage("Tipo de camiseta deve ser 'Regular' ou 'Over'.");
    }
}
