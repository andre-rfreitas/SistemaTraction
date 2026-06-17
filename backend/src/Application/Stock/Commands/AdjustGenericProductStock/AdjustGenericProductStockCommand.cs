using MediatR;

namespace SistemaTraction.Application.Stock.Commands.AdjustGenericProductStock;

public record AdjustGenericProductStockCommand(
    Guid ProductId,
    string AdjustmentType, // "Entrada" ou "Saída"
    int Quantity,
    string Reason,
    decimal UnitCost = 0m
) : IRequest<Guid>;
