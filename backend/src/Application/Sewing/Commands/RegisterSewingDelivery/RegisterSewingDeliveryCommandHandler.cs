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

        var alreadyFullDelivered = await context.SewingDeliveries
            .AnyAsync(s => s.CuttingOrderId == request.OrderId && !s.IsPartial, cancellationToken);
        if (alreadyFullDelivered)
            throw new DomainException("Este pedido já possui entrega completa do costureiro registrada.");

        var hasActiveSewer = await context.Sewers.AnyAsync(s => s.IsActive, cancellationToken);
        if (!hasActiveSewer)
            throw new DomainException("Nenhuma costureira cadastrada. Cadastre uma costureira antes de registrar a entrega.");

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

        // Aggregate totals
        var allGood = AggregatePieces(request.Items.Select(i => i.GoodPieces));
        var allDefective = AggregatePieces(request.Items.Select(i => i.DefectivePieces));

        var sewingCostTotal = allGood
            .Where(kv => kv.Value > 0)
            .Sum(kv => (decimal)kv.Value * SewingPrice(kv.Key));

        // Pre-calculate defect cost per roll so we can pass the total to SewingDelivery.Create()
        decimal defectCostTotal = 0m;
        foreach (var input in request.Items)
        {
            var orderItem = order.Items.FirstOrDefault(oi => oi.FabricRollId == input.FabricRollId);
            if (orderItem is null) continue;

            var itemTotalPieces = input.GoodPieces.Values.Sum() + input.DefectivePieces.Values.Sum();
            var fabricCostPerPiece = itemTotalPieces > 0 ? orderItem.FabricRoll!.PriceTotal / itemTotalPieces : 0m;

            defectCostTotal += input.DefectivePieces
                .Where(kv => kv.Value > 0)
                .Sum(kv => (decimal)kv.Value * (fabricCostPerPiece + cuttingPrice + SewingPrice(kv.Key)));
        }

        var sewingDelivery = SewingDelivery.Create(
            request.OrderId,
            allGood,
            allDefective,
            sewingCostTotal,
            defectCostTotal);
        context.SewingDeliveries.Add(sewingDelivery);

        // Per-roll: add stock to the correct color
        var itemResults = new List<SewingItemResult>();

        foreach (var input in request.Items)
        {
            var orderItem = order.Items.FirstOrDefault(oi => oi.FabricRollId == input.FabricRollId);
            if (orderItem is null) continue;

            var roll = orderItem.FabricRoll!;
            var fabricColorId = roll.FabricColorId;
            var colorName = roll.FabricColor!.Name;
            var hexCode = roll.FabricColor.HexCode;
            var typeName = roll.FabricType!.Name;
            var typeVariation = roll.FabricType.Variation;

            foreach (var (size, qty) in input.GoodPieces.Where(kv => kv.Value > 0))
            {
                var normalizedSize = size.ToUpper();
                var stockItem = await context.StockItems
                    .FirstOrDefaultAsync(s => s.FabricColorId == fabricColorId && s.Size == normalizedSize, cancellationToken);

                if (stockItem is null)
                {
                    stockItem = StockItem.Create(fabricColorId, colorName, typeName, typeVariation, normalizedSize, qty, "REG");
                    context.StockItems.Add(stockItem);
                }
                else
                {
                    stockItem.AddStock(qty);
                }

                context.ShirtStockMovements.Add(ShirtStockMovement.Create(
                    stockItem.Id, fabricColorId, colorName, normalizedSize,
                    qty, $"Costura pedido #{order.OrderNumber}", "Costureiro", order.Id));
            }

            var itemGoodTotal = input.GoodPieces.Values.Sum();
            var itemDefectiveTotal = input.DefectivePieces.Values.Sum();
            if (itemGoodTotal > 0 || itemDefectiveTotal > 0)
            {
                itemResults.Add(new SewingItemResult(
                    colorName,
                    typeName,
                    hexCode,
                    input.GoodPieces.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value),
                    input.DefectivePieces.Where(kv => kv.Value > 0).ToDictionary(kv => kv.Key, kv => kv.Value)));
            }
        }

        if (sewingCostTotal > 0)
        {
            context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                "Costura",
                sewingCostTotal,
                $"Costura pedido #{order.OrderNumber} — {allGood.Values.Sum()} peças",
                sewingDelivery.Id,
                "SewingDelivery"));
        }

        if (defectCostTotal > 0)
        {
            context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                "Defeitos",
                defectCostTotal,
                $"Defeitos pedido #{order.OrderNumber} — {allDefective.Values.Sum()} peça(s) defeituosa(s)",
                sewingDelivery.Id,
                "SewingDelivery"));
        }

        order.MarkSewingDelivered();

        await context.SaveChangesAsync(cancellationToken);

        return new RegisterSewingDeliveryResult(
            sewingDelivery.Id,
            allGood.Values.Sum(),
            allDefective.Values.Sum(),
            sewingCostTotal,
            defectCostTotal,
            allGood,
            allDefective,
            itemResults);
    }

    private static Dictionary<string, int> AggregatePieces(IEnumerable<Dictionary<string, int>> sources)
    {
        var result = new Dictionary<string, int>();
        foreach (var dict in sources)
            foreach (var (size, qty) in dict)
                result[size] = result.GetValueOrDefault(size) + qty;
        return result;
    }

    private static decimal GetDecimalConfig(List<AppConfig> configs, string key, decimal fallback)
    {
        var cfg = configs.FirstOrDefault(c => c.Key == key);
        if (cfg is null) return fallback;
        return decimal.TryParse(cfg.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var val) ? val : fallback;
    }
}
