using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Financial.Commands.ReverseFinancialEntry;

public class ReverseFinancialEntryCommandHandler(IApplicationDbContext context)
    : IRequestHandler<ReverseFinancialEntryCommand, ReverseFinancialEntryResult>
{
    public async Task<ReverseFinancialEntryResult> Handle(ReverseFinancialEntryCommand request, CancellationToken cancellationToken)
    {
        var original = await context.FinancialEntries
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
            ?? throw new DomainException("Lançamento financeiro não encontrado.");

        var alreadyReversed = await context.FinancialEntries
            .AnyAsync(e => e.IsReversal && e.ReferenceId == original.Id, cancellationToken);

        if (alreadyReversed)
            throw new DomainException("Este lançamento já foi estornado.");

        if (original.ReferenceType == "CuttingDelivery" && original.ReferenceId.HasValue)
            await ReverseCuttingDelivery(original.ReferenceId.Value, cancellationToken);
        else if (original.ReferenceType == "SewingDelivery" && original.ReferenceId.HasValue)
            await ReverseSewingDelivery(original.Id, original.ReferenceId.Value, cancellationToken);

        // Sempre cria o estorno do lançamento disparador — para SewingDelivery os outros
        // lançamentos vinculados são estornados dentro de ReverseSewingDelivery.
        var reversal = FinancialEntry.CreateReversal(original);
        context.FinancialEntries.Add(reversal);
        await context.SaveChangesAsync(cancellationToken);

        return new ReverseFinancialEntryResult(reversal.Id);
    }

    private async Task ReverseCuttingDelivery(Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await context.CuttingDeliveries
            .FirstOrDefaultAsync(d => d.Id == deliveryId, cancellationToken)
            ?? throw new DomainException("Entrega de corte não encontrada.");

        var hasSewing = await context.SewingDeliveries
            .AnyAsync(s => s.CuttingOrderId == delivery.CuttingOrderId && !s.IsDeleted, cancellationToken);

        if (hasSewing)
            throw new DomainException("Estorno bloqueado: já existe entrega de costura para este pedido. Estorne a costura primeiro.");

        var order = await context.CuttingOrders
            .Include(o => o.Items).ThenInclude(i => i.FabricRoll)
            .FirstOrDefaultAsync(o => o.Id == delivery.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        foreach (var item in order.Items)
            item.FabricRoll?.RevertToAvailable();

        order.CancelDelivered();
    }

    private async Task ReverseSewingDelivery(Guid triggeredEntryId, Guid deliveryId, CancellationToken cancellationToken)
    {
        var delivery = await context.SewingDeliveries
            .FirstOrDefaultAsync(s => s.Id == deliveryId, cancellationToken)
            ?? throw new DomainException("Entrega de costura não encontrada.");

        var order = await context.CuttingOrders
            .FirstOrDefaultAsync(o => o.Id == delivery.CuttingOrderId && !o.IsDeleted, cancellationToken)
            ?? throw new DomainException("Pedido de corte não encontrado.");

        // Estornar os outros lançamentos do mesmo SewingDelivery (o disparador é estornado no Handle)
        var otherLinkedEntries = await context.FinancialEntries
            .Where(e => e.ReferenceId == deliveryId && e.ReferenceType == "SewingDelivery"
                        && e.Id != triggeredEntryId && !e.IsReversal)
            .ToListAsync(cancellationToken);

        foreach (var entry in otherLinkedEntries)
        {
            var alreadyReversed = await context.FinancialEntries
                .AnyAsync(e => e.IsReversal && e.ReferenceId == entry.Id, cancellationToken);
            if (!alreadyReversed)
                context.FinancialEntries.Add(FinancialEntry.CreateReversal(entry));
        }

        // Reverter estoque de camisetas
        var movements = await context.ShirtStockMovements
            .Where(m => m.ReferenceId == order.Id && m.Delta > 0)
            .ToListAsync(cancellationToken);

        foreach (var movement in movements)
        {
            var stockItem = await context.StockItems.FindAsync(movement.StockItemId);
            if (stockItem is null) continue;

            stockItem.UseFromStock(movement.Delta);
            context.ShirtStockMovements.Add(ShirtStockMovement.Create(
                movement.StockItemId,
                movement.FabricColorId,
                movement.FabricColorName,
                movement.Size,
                -movement.Delta,
                $"Estorno costura pedido #{order.OrderNumber}",
                "Estorno",
                order.Id));
        }

        order.CancelDelivered();
    }
}
