using MediatR;
using SistemaTraction.Application.Stock.DTOs;

namespace SistemaTraction.Application.Stock.Commands.AdjustShirtStock;

public record AdjustShirtStockCommand(
    Guid FabricColorId,
    string Size,
    string AdjustmentType,   // "Entrada" | "Saída"
    int Quantity,
    string Reason,
    string ShirtType = "Regular",
    decimal UnitCost = 0m    // se > 0 e AdjustmentType == "Entrada", gera lançamento financeiro
) : IRequest<AdjustShirtStockResult>;
