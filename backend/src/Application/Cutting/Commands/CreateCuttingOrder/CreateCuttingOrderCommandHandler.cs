using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Cutting.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Cutting.Commands.CreateCuttingOrder;

public class CreateCuttingOrderCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateCuttingOrderCommand, CreateCuttingOrderResult>
{
    public async Task<CreateCuttingOrderResult> Handle(CreateCuttingOrderCommand request, CancellationToken cancellationToken)
    {
        var roll = await context.FabricRolls
            .Include(r => r.FabricType)
            .Include(r => r.FabricColor)
            .FirstOrDefaultAsync(r => r.Id == request.FabricRollId && !r.IsDeleted, cancellationToken)
            ?? throw new DomainException("Bobina não encontrada.");

        if (roll.Status != FabricRollStatus.Available)
            throw new DomainException("Apenas bobinas disponíveis podem ser enviadas ao corte.");

        var orderNumber = await context.CuttingOrders.AnyAsync(cancellationToken)
            ? await context.CuttingOrders.MaxAsync(o => o.OrderNumber, cancellationToken) + 1
            : 1;

        var order = CuttingOrder.Create(orderNumber, request.FabricRollId, request.RequestedPieces, request.Notes);

        if (request.RecommendedPieces is not null)
            order.SetRecommendationSnapshot(request.RecommendedPieces, request.RecommendationDays, request.RecommendationBasedOnOrders);

        context.CuttingOrders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        var sizes = await context.AppConfigs
            .Where(c => c.Key == "sizes_available" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "P,M,G,G1,GG";

        var cutterPhone = await context.AppConfigs
            .Where(c => c.Key == "wp_cutter_phone" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "";

        var cutterName = await context.AppConfigs
            .Where(c => c.Key == "wp_cutter_name" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "Cortador";

        var template = await context.AppConfigs
            .Where(c => c.Key == "wp_template_cutter" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken);

        var message = BuildWhatsAppMessage(order, roll, sizes.Split(','), template);

        string? waMeLink = null;
        if (!string.IsNullOrWhiteSpace(cutterPhone))
        {
            var cleanPhone = new string(cutterPhone.Where(char.IsDigit).ToArray());
            var encoded = Uri.EscapeDataString(message);
            waMeLink = $"https://wa.me/{cleanPhone}?text={encoded}";
        }

        return new CreateCuttingOrderResult(order.Id, order.OrderNumber, message, waMeLink, cutterPhone, cutterName);
    }

    private static string BuildWhatsAppMessage(CuttingOrder order, FabricRoll roll, string[] sizeOrder, string? template)
    {
        var pieces = order.GetRequestedPieces();
        var sizesBlock = string.Join("\n", sizeOrder
            .Where(s => pieces.TryGetValue(s, out var q) && q > 0)
            .Select(s => $"{pieces[s]} {s}"));
        var total = order.GetTotalPieces();

        if (!string.IsNullOrWhiteSpace(template))
        {
            return template
                .Replace("\\n", "\n")
                .Replace("{OrderNumber}", order.OrderNumber.ToString())
                .Replace("{Color}", roll.FabricColor!.Name)
                .Replace("{Variation}", roll.FabricType!.Variation)
                .Replace("{SizesBlock}", sizesBlock)
                .Replace("{Total}", total.ToString());
        }

        var lines = new List<string>
        {
            $"Pedido #{order.OrderNumber}",
            $"{roll.FabricColor!.Name} {roll.FabricType!.Variation}"
        };
        foreach (var size in sizeOrder)
            if (pieces.TryGetValue(size, out var qty) && qty > 0)
                lines.Add($"{qty} {size}");
        lines.Add($"Total: {total} peças");
        return string.Join("\n", lines);
    }
}
