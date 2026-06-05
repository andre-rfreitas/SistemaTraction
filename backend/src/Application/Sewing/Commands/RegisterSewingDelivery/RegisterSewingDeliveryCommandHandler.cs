using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Sewing.Commands.RegisterSewingDelivery;

public class RegisterSewingDeliveryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterSewingDeliveryCommand, RegisterSewingDeliveryResult>
{
    public async Task<RegisterSewingDeliveryResult> Handle(
        RegisterSewingDeliveryCommand request,
        CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricColor)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido não encontrado.");

        if (order.Status != CuttingOrderStatus.Delivered)
            throw new DomainException("Apenas pedidos no status 'Entregue pelo cortador' podem ter entrega do costureiro registrada.");

        var alreadyDelivered = await context.SewingDeliveries
            .AnyAsync(s => s.CuttingOrderId == request.OrderId, cancellationToken);
        if (alreadyDelivered)
            throw new DomainException("Este pedido já possui entrega do costureiro registrada.");

        var cuttingDelivery = await context.CuttingDeliveries
            .FirstOrDefaultAsync(d => d.CuttingOrderId == request.OrderId, cancellationToken)
            ?? throw new DomainException("Entrega do cortador não encontrada para este pedido.");

        var configs = await context.AppConfigs
            .Where(c => !c.IsDeleted && (
                c.Key == "sewing_price_default" ||
                c.Key == "sewing_price_g1" ||
                c.Key == "cutting_price_default"))
            .ToListAsync(cancellationToken);

        var sewingPriceDefault = GetDecimalConfig(configs, "sewing_price_default", 5.60m);
        var sewingPriceG1 = GetDecimalConfig(configs, "sewing_price_g1", 6.30m);
        var cuttingPrice = GetDecimalConfig(configs, "cutting_price_default", 1.00m);

        decimal SewingPrice(string size) =>
            size.Equals("G1", StringComparison.OrdinalIgnoreCase) ? sewingPriceG1 : sewingPriceDefault;

        var sewingCostTotal = request.GoodPieces
            .Where(kv => kv.Value > 0)
            .Sum(kv => (decimal)kv.Value * SewingPrice(kv.Key));

        var totalDeliveredPieces = cuttingDelivery.GetTotalPieces();
        var totalFabricPrice = order.Items.Sum(i => i.FabricRoll!.PriceTotal);
        var fabricCostPerPiece = totalDeliveredPieces > 0
            ? totalFabricPrice / totalDeliveredPieces
            : 0m;

        var defectCostTotal = request.DefectivePieces
            .Where(kv => kv.Value > 0)
            .Sum(kv => (decimal)kv.Value * (fabricCostPerPiece + cuttingPrice + SewingPrice(kv.Key)));

        var sewingDelivery = SewingDelivery.Create(
            request.OrderId,
            request.GoodPieces,
            request.DefectivePieces,
            sewingCostTotal,
            defectCostTotal);
        context.SewingDeliveries.Add(sewingDelivery);

        // Use first item's color for stock — covers the common case where all rolls share the same color
        var firstItem = order.Items.First();
        var fabricColorId = firstItem.FabricRoll!.FabricColorId;
        var colorName = firstItem.FabricRoll!.FabricColor!.Name;
        var typeName = firstItem.FabricRoll!.FabricType!.Name;
        var typeVariation = firstItem.FabricRoll!.FabricType!.Variation;
        var orderNum = order.OrderNumber;

        foreach (var (size, qty) in request.GoodPieces.Where(kv => kv.Value > 0))
        {
            var normalizedSize = size.ToUpper();
            var stockItem = await context.StockItems
                .FirstOrDefaultAsync(s => s.FabricColorId == fabricColorId && s.Size == normalizedSize, cancellationToken);

            if (stockItem is null)
            {
                stockItem = StockItem.Create(fabricColorId, colorName, typeName, typeVariation, normalizedSize, qty);
                context.StockItems.Add(stockItem);
            }
            else
            {
                stockItem.AddStock(qty);
            }

            context.ShirtStockMovements.Add(ShirtStockMovement.Create(
                stockItem.Id, fabricColorId, colorName, normalizedSize,
                qty, $"Costura pedido #{orderNum}", "Costureiro", order.Id));
        }

        var totalGood = request.GoodPieces.Where(kv => kv.Value > 0).Sum(kv => kv.Value);
        var totalDefective = request.DefectivePieces.Where(kv => kv.Value > 0).Sum(kv => kv.Value);

        if (sewingCostTotal > 0)
        {
            context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                "Costura",
                sewingCostTotal,
                $"Costura pedido #{orderNum} — {totalGood} peças",
                sewingDelivery.Id,
                "SewingDelivery"));
        }

        if (defectCostTotal > 0)
        {
            context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                "Defeitos",
                defectCostTotal,
                $"Defeitos pedido #{orderNum} — {totalDefective} peça(s) defeituosa(s)",
                sewingDelivery.Id,
                "SewingDelivery"));
        }

        order.MarkSewingDelivered();

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterSewingDeliveryResult(
            sewingDelivery.Id,
            totalGood,
            totalDefective,
            sewingCostTotal,
            defectCostTotal,
            request.GoodPieces,
            request.DefectivePieces);
    }

    private static decimal GetDecimalConfig(List<AppConfig> configs, string key, decimal fallback)
    {
        var cfg = configs.FirstOrDefault(c => c.Key == key);
        if (cfg is null) return fallback;
        return decimal.TryParse(cfg.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }
}
