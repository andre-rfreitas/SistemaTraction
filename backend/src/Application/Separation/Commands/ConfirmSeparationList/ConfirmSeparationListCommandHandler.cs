using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;
using SistemaTraction.Domain.Stock;

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
            .GroupBy(i => (
                ModelCode: (i.Sku.Split('-').FirstOrDefault() ?? "REG").ToUpper(),
                ColorLow: i.Color.ToLower(), 
                SizeUp: i.Size.ToUpper()
            ))
            .ToList();

        var shirtDeductions = new List<ShirtDeductionDto>();

        foreach (var group in shirtGroups)
        {
            var (modelCode, colorLow, sizeUp) = group.Key;
            var needed = group.Sum(i => i.Quantity);
            var colorStr = group.First().Color;

            var stockItem = await context.StockItems
                .FirstOrDefaultAsync(s =>
                    s.ModelCode == modelCode &&
                    s.FabricColorName.ToLower() == colorLow && 
                    s.Size == sizeUp && 
                    !s.IsDeleted,
                    cancellationToken);

            // Fallback para itens antigos sem model code exato
            if (stockItem is null)
            {
                stockItem = await context.StockItems
                    .FirstOrDefaultAsync(s =>
                        s.FabricColorName.ToLower() == colorLow && 
                        s.Size == sizeUp && 
                        !s.IsDeleted,
                        cancellationToken);
            }

            if (stockItem is null)
                throw new DomainException(
                    $"Sem registro de estoque para '{colorStr} {sizeUp} ({modelCode})'. Registre a entrada no estoque primeiro.");

            stockItem.UseFromStock(needed);
            shirtDeductions.Add(new ShirtDeductionDto(modelCode, colorStr, sizeUp, needed));

            context.ShirtStockMovements.Add(ShirtStockMovement.Create(
                stockItem.Id, stockItem.FabricColorId, colorStr, sizeUp,
                -needed, $"Lista de separação confirmada", "Separação", request.SeparationListId, modelCode));
        }

        // ── 2. Confirm list ────────────────────────────────────────────────────
        list.Confirm();

        await context.SaveChangesAsync(cancellationToken);

        return new SeparationConfirmResultDto(
            list.Id,
            shirtDeductions);
    }
}
