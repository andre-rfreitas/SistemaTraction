using System.Globalization;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.ConfirmSeparationList;

public class ConfirmSeparationListCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ConfirmSeparationListCommand, SeparationConfirmResultDto>
{
    public async Task<SeparationConfirmResultDto> Handle(
        ConfirmSeparationListCommand request, CancellationToken cancellationToken)
    {
        var list = await context.SeparationLists
            .FirstOrDefaultAsync(l => l.Id == request.SeparationListId && !l.IsDeleted, cancellationToken)
            ?? throw new DomainException("Lista não encontrada.");

        if (list.Status != SeparationListStatus.Pending)
            throw new DomainException("Apenas listas pendentes podem ser confirmadas.");

        var items = await context.SeparationItems
            .Where(i => i.SeparationListId == request.SeparationListId && !i.IsDeleted)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
            throw new DomainException("A lista não contém itens.");

        // ── 1. Deduct shirt stock ──────────────────────────────────────────────
        var shirtGroups = items
            .GroupBy(i => (i.Color.ToLower(), i.Size.ToUpper()))
            .ToList();

        var shirtDeductions = new List<ShirtDeductionDto>();

        foreach (var group in shirtGroups)
        {
            var (color, size) = group.Key;
            var needed = group.Sum(i => i.Quantity);
            var colorStr = group.First().Color;

            var stockItem = await context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.FabricColorName.ToLower() == color && s.Size == size && !s.IsDeleted,
                    cancellationToken)
                ?? throw new DomainException(
                    $"Sem registro de estoque para '{colorStr} {size}'. Registre a entrega do costureiro primeiro.");

            stockItem.UseFromStock(needed);
            shirtDeductions.Add(new ShirtDeductionDto(colorStr, size, needed));
        }

        // ── 2. Process DTF stock ───────────────────────────────────────────────
        var dtfGroups = items
            .Where(i => i.DtfModelId.HasValue)
            .GroupBy(i => i.DtfModelId!.Value)
            .ToList();

        var dtfUsages = new List<DtfUsageDto>();
        var dtfOrders = new List<DtfOrderDto>();
        var totalDtfCost = 0m;

        foreach (var group in dtfGroups)
        {
            var modelId = group.Key;
            var needed = group.Sum(i => i.Quantity);

            var model = await context.DtfModels
                .FirstOrDefaultAsync(m => m.Id == modelId && !m.IsDeleted, cancellationToken)
                ?? throw new DomainException($"Modelo DTF não encontrado: {modelId}.");

            var stockItem = await context.DtfStockItems
                .Include(s => s.Movements)
                .FirstOrDefaultAsync(s => s.DtfModelId == modelId && !s.IsDeleted, cancellationToken);

            var available = stockItem?.CurrentQuantity ?? 0;

            if (available >= needed)
            {
                // Enough in stock — just deduct
                stockItem!.AddMovement(DtfMovementType.Saida, needed, $"Lista separação #{request.SeparationListId}");
                dtfUsages.Add(new DtfUsageDto(model.Name, needed));
            }
            else
            {
                // Need to order sheets
                var deficit = needed - available;
                var sheetsToOrder = (int)Math.Ceiling((double)deficit / model.StampsPerSheet);
                var stampsGained = sheetsToOrder * model.StampsPerSheet;
                var orderCost = sheetsToOrder * model.SheetCost;
                totalDtfCost += orderCost;

                // Create stock item if it doesn't exist yet
                if (stockItem is null)
                {
                    stockItem = DtfStockItem.Create(modelId);
                    context.DtfStockItems.Add(stockItem);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // Entrada: full sheet(s)
                stockItem.AddMovement(DtfMovementType.Entrada, stampsGained,
                    $"Compra {sheetsToOrder} folha(s) para lista #{request.SeparationListId}");

                // Saida: exact quantity used
                stockItem.AddMovement(DtfMovementType.Saida, needed,
                    $"Uso lista separação #{request.SeparationListId}");

                context.FinancialEntries.Add(FinancialEntry.CreateExpense(
                    "DTF",
                    orderCost,
                    $"DTF {model.Name} — {sheetsToOrder} folha(s) para lista de separação",
                    request.SeparationListId,
                    "SeparationList"));

                dtfOrders.Add(new DtfOrderDto(
                    model.Name, model.SheetLabel, model.StampsPerSheet,
                    sheetsToOrder, model.SheetCost, orderCost));
            }
        }

        // ── 3. Confirm list ────────────────────────────────────────────────────
        list.Confirm();

        await context.SaveChangesAsync(cancellationToken);

        // ── 4. Build WhatsApp message for DTF supplier ─────────────────────────
        var dtfPhone = await context.AppConfigs
            .Where(c => c.Key == "wp_dtf_phone" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "";

        var dtfName = await context.AppConfigs
            .Where(c => c.Key == "wp_dtf_name" && !c.IsDeleted)
            .Select(c => c.Value)
            .FirstOrDefaultAsync(cancellationToken) ?? "Fornecedor DTF";

        string? whatsAppMessage = null;
        string? waMeLink = null;

        if (dtfOrders.Count > 0)
        {
            whatsAppMessage = BuildDtfMessage(dtfOrders, totalDtfCost);
            if (!string.IsNullOrWhiteSpace(dtfPhone))
            {
                var cleanPhone = new string(dtfPhone.Where(char.IsDigit).ToArray());
                waMeLink = $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(whatsAppMessage)}";
            }
        }

        return new SeparationConfirmResultDto(
            list.Id,
            shirtDeductions,
            dtfUsages,
            dtfOrders,
            totalDtfCost,
            whatsAppMessage,
            waMeLink,
            dtfName,
            dtfPhone);
    }

    private static string BuildDtfMessage(List<DtfOrderDto> orders, decimal total)
    {
        var date = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        var lines = new List<string> { $"Pedido DTF - {date}" };

        foreach (var o in orders)
            lines.Add($"{o.SheetLabel} — {o.ModelName} ({o.StampsPerSheet} estampas/folha) — {o.SheetsOrdered} folha(s)");

        var totalSheets = orders.Sum(o => o.SheetsOrdered);
        var totalFormatted = total.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
        lines.Add($"\nTotal: {totalSheets} folha(s) — R${totalFormatted}");

        return string.Join("\n", lines);
    }
}
