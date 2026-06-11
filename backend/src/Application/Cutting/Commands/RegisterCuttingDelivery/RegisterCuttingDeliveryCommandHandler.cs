using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Cutting.Commands.RegisterCuttingDelivery;

public class RegisterCuttingDeliveryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RegisterCuttingDeliveryCommand, RegisterCuttingDeliveryResult>
{
    public async Task<RegisterCuttingDeliveryResult> Handle(RegisterCuttingDeliveryCommand request, CancellationToken cancellationToken)
    {
        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll).ThenInclude(r => r!.FabricColor)
            .FirstOrDefaultAsync(o => o.Id == request.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        if (order.Status != CuttingOrderStatus.SentToCutter)
            throw new DomainException("Apenas pedidos enviados ao cortador podem receber entrega.");

        var rollIds = request.Items.Select(i => i.FabricRollId).ToHashSet();
        var unknownRoll = rollIds.FirstOrDefault(id => !order.Items.Any(oi => oi.FabricRollId == id));
        if (unknownRoll != default)
            throw new DomainException("Bobina não pertence a este pedido.");

        var aggregated = AggregatePieces(request.Items);
        var totalPieces = aggregated.Values.Sum();

        var cuttingPrice = await context.AppConfigs
            .Where(c => c.Key == "cutting_price_default" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "1.00";

        var pricePerPiece = decimal.Parse(cuttingPrice, CultureInfo.InvariantCulture);
        var cuttingCostTotal = totalPieces * pricePerPiece;

        var delivery = CuttingDelivery.Create(request.CuttingOrderId, aggregated, cuttingCostTotal);
        context.CuttingDeliveries.Add(delivery);

        var rollsSummary = string.Join(", ", order.Items.Select(i =>
            $"{i.FabricRoll!.FabricType!.Variation} {i.FabricRoll.FabricColor!.Name}"));
        var description = $"Corte Pedido #{order.OrderNumber} — {rollsSummary} — {totalPieces} peças";
        var entry = FinancialEntry.CreateExpense("Corte", cuttingCostTotal, description, delivery.Id, "CuttingDelivery");
        context.FinancialEntries.Add(entry);

        order.MarkDelivered();
        foreach (var item in order.Items)
            item.FabricRoll!.MarkConsumed();

        await context.SaveChangesAsync(cancellationToken);

        var sewerPhone = await context.AppConfigs
            .Where(c => c.Key == "wp_sewer_phone" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "";

        var sewerName = await context.AppConfigs
            .Where(c => c.Key == "wp_sewer_name" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "Costureiro";

        var sizes = await context.AppConfigs
            .Where(c => c.Key == "sizes_available" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "P,M,G,G1,GG";

        var message = BuildSewerMessage(order, request.Items, sizes.Split(','));

        string? waMeLink = null;
        if (!string.IsNullOrWhiteSpace(sewerPhone))
        {
            var cleanPhone = new string(sewerPhone.Where(char.IsDigit).ToArray());
            waMeLink = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";
        }

        return new RegisterCuttingDeliveryResult(
            delivery.Id, totalPieces, cuttingCostTotal, message, waMeLink, sewerPhone, sewerName);
    }

    private static Dictionary<string, int> AggregatePieces(List<RegisterCuttingDeliveryItemInput> items)
    {
        var result = new Dictionary<string, int>();
        foreach (var item in items)
            foreach (var (size, qty) in item.DeliveredPieces)
                result[size] = result.GetValueOrDefault(size) + qty;
        return result;
    }

    private static string BuildSewerMessage(
        CuttingOrder order,
        List<RegisterCuttingDeliveryItemInput> items,
        string[] sizeOrder)
    {
        var lines = new List<string> { $"Pedido {order.OrderNumber}" };

        var grandTotalByType = new Dictionary<string, int>();
        var grandTotal = 0;

        foreach (var requestItem in items)
        {
            var orderItem = order.Items.FirstOrDefault(oi => oi.FabricRollId == requestItem.FabricRollId);
            if (orderItem is null) continue;

            var colorName = orderItem.FabricRoll!.FabricColor!.Name;
            var typeName = orderItem.FabricRoll.FabricType!.Name;
            var subtotal = requestItem.DeliveredPieces.Values.Sum();
            if (subtotal == 0) continue;

            lines.Add(string.Empty);
            lines.Add($"Camisetas {colorName} {typeName}");

            foreach (var size in sizeOrder)
                if (requestItem.DeliveredPieces.TryGetValue(size, out var qty) && qty > 0)
                    lines.Add($"{qty} {size}");

            lines.Add($"Total {subtotal}");

            grandTotalByType[typeName] = grandTotalByType.GetValueOrDefault(typeName) + subtotal;
            grandTotal += subtotal;
        }

        lines.Add(string.Empty);

        if (grandTotalByType.Count == 1)
        {
            var (typeName, typeTotal) = grandTotalByType.First();
            lines.Add($"Total camisetas {typeName}: {typeTotal}");
        }
        else
        {
            foreach (var (typeName, typeTotal) in grandTotalByType)
                lines.Add($"Total {typeName}: {typeTotal}");
            lines.Add($"Total geral: {grandTotal} camisetas");
        }

        return string.Join("\n", lines);
    }
}
