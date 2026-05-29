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
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricType)
            .Include(o => o.FabricRoll).ThenInclude(r => r!.FabricColor)
            .FirstOrDefaultAsync(o => o.Id == request.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        if (order.Status != CuttingOrderStatus.SentToCutter)
            throw new DomainException("Apenas pedidos enviados ao cortador podem receber entrega.");

        var cuttingPrice = await context.AppConfigs
            .Where(c => c.Key == "cutting_price_default" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "1.00";

        var pricePerPiece = decimal.Parse(cuttingPrice, CultureInfo.InvariantCulture);
        var totalPieces = request.DeliveredPieces.Values.Sum();
        var cuttingCostTotal = totalPieces * pricePerPiece;

        var delivery = CuttingDelivery.Create(request.CuttingOrderId, request.DeliveredPieces, cuttingCostTotal);
        context.CuttingDeliveries.Add(delivery);

        var description = $"Corte Pedido #{order.OrderNumber} — {order.FabricRoll!.FabricColor!.Name} {order.FabricRoll.FabricType!.Variation} — {totalPieces} peças";
        var entry = FinancialEntry.CreateExpense("Corte", cuttingCostTotal, description, delivery.Id, "CuttingDelivery");
        context.FinancialEntries.Add(entry);

        order.MarkDelivered();
        order.FabricRoll!.MarkConsumed();

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

        var message = BuildSewerMessage(order, delivery, sizes.Split(','));

        string? waMeLink = null;
        if (!string.IsNullOrWhiteSpace(sewerPhone))
        {
            var cleanPhone = new string(sewerPhone.Where(char.IsDigit).ToArray());
            waMeLink = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(message)}";
        }

        return new RegisterCuttingDeliveryResult(
            delivery.Id, totalPieces, cuttingCostTotal, message, waMeLink, sewerPhone, sewerName);
    }

    private static string BuildSewerMessage(CuttingOrder order, CuttingDelivery delivery, string[] sizeOrder)
    {
        var pieces = delivery.GetDeliveredPieces();
        var roll = order.FabricRoll!;
        var totalPieces = delivery.GetTotalPieces();
        var costFormatted = delivery.CuttingCostTotal.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

        var lines = new List<string> { $"Pedido {order.OrderNumber}" };

        // One group per delivery (one color+variation)
        lines.Add($"{roll.FabricColor!.Name} {roll.FabricType!.Variation} - {totalPieces}");
        foreach (var size in sizeOrder)
        {
            if (pieces.TryGetValue(size, out var qty) && qty > 0)
                lines.Add($"{qty} {size}");
        }

        lines.Add($"Total {totalPieces} camisetas R${costFormatted}");
        return string.Join("\n", lines);
    }
}
