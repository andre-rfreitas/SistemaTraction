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
        var rollIds = request.Items.Select(i => i.FabricRollId).ToList();
        var rolls = await context.FabricRolls
            .Include(r => r.FabricType)
            .Include(r => r.FabricColor)
            .Where(r => rollIds.Contains(r.Id) && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        if (rolls.Count != rollIds.Count)
            throw new DomainException("Uma ou mais bobinas não foram encontradas.");

        var unavailable = rolls.FirstOrDefault(r => r.Status != FabricRollStatus.Available);
        if (unavailable is not null)
            throw new DomainException($"A bobina {unavailable.FabricColor!.Name} {unavailable.FabricType!.Variation} não está disponível para corte.");

        var orderNumber = await context.CuttingOrders.AnyAsync(cancellationToken)
            ? await context.CuttingOrders.MaxAsync(o => o.OrderNumber, cancellationToken) + 1
            : 1;

        var itemInputs = request.Items
            .Select(i => (i.FabricRollId, i.RequestedPieces))
            .ToList();

        var order = CuttingOrder.Create(orderNumber, itemInputs, request.Notes);

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

        var rollMap = rolls.ToDictionary(r => r.Id);
        var message = BuildWhatsAppMessage(order, request.Items, rollMap, sizes.Split(','), template);

        string? waMeLink = null;
        if (!string.IsNullOrWhiteSpace(cutterPhone))
        {
            var cleanPhone = new string(cutterPhone.Where(char.IsDigit).ToArray());
            var encoded = Uri.EscapeDataString(message);
            waMeLink = $"https://wa.me/{cleanPhone}?text={encoded}";
        }

        return new CreateCuttingOrderResult(order.Id, order.OrderNumber, message, waMeLink, cutterPhone, cutterName);
    }

    private static string BuildWhatsAppMessage(
        CuttingOrder order,
        List<CreateCuttingOrderItemInput> items,
        Dictionary<Guid, FabricRoll> rollMap,
        string[] sizeOrder,
        string? template)
    {
        if (items.Count == 1 && !string.IsNullOrWhiteSpace(template))
        {
            var roll = rollMap[items[0].FabricRollId];
            var pieces = items[0].RequestedPieces;
            var sizesBlock = string.Join("\n", sizeOrder
                .Where(s => pieces.TryGetValue(s, out var q) && q > 0)
                .Select(s => $"{pieces[s]} {s}"));
            var total = pieces.Values.Sum();

            return template
                .Replace("\\n", "\n")
                .Replace("{OrderNumber}", order.OrderNumber.ToString())
                .Replace("{Color}", roll.FabricColor!.Name)
                .Replace("{Variation}", roll.FabricType!.Variation)
                .Replace("{SizesBlock}", sizesBlock)
                .Replace("{Total}", total.ToString());
        }

        var lines = new List<string> { $"Pedido #{order.OrderNumber}" };
        var grandTotal = 0;

        foreach (var item in items)
        {
            var roll = rollMap[item.FabricRollId];
            lines.Add($"{roll.FabricColor!.Name} {roll.FabricType!.Variation}");
            foreach (var size in sizeOrder)
                if (item.RequestedPieces.TryGetValue(size, out var qty) && qty > 0)
                    lines.Add($"  {qty} {size}");
            var subtotal = item.RequestedPieces.Values.Sum();
            grandTotal += subtotal;
        }

        lines.Add($"Total: {grandTotal} peças");
        return string.Join("\n", lines);
    }
}
