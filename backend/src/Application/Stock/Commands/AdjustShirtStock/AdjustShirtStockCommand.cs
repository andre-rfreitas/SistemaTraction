using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Commands.AdjustShirtStock;

public record AdjustShirtStockCommand(
    Guid FabricColorId,
    string Size,
    string AdjustmentType,   // "Entrada" | "Saída"
    int Quantity,
    string Reason,
    string ShirtType = "Regular"
) : IRequest<AdjustShirtStockResult>;
