using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.UploadSeparationList;

public class UploadSeparationListCommandHandler(IApplicationDbContext context, IPdfParser pdfParser)
    : IRequestHandler<UploadSeparationListCommand, SeparationListDetailDto>
{
    public async Task<SeparationListDetailDto> Handle(
        UploadSeparationListCommand request, CancellationToken cancellationToken)
    {
        var parsed = pdfParser.Parse(request.PdfStream, request.FileName);
        if (!parsed.Success)
            throw new DomainException(
                $"Não foi possível processar o PDF: {parsed.ErrorMessage ?? "formato não reconhecido"}. " +
                "Verifique se o arquivo é uma lista de separação válida do ERP e se os Códigos SKU estão configurados.");

        // Load SKU code mappings (Modelo, Estampa, Cor, Tamanho)
        var skuCodes = await context.SkuCodes
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var skuLookup = skuCodes.ToLookup(c => c.Code, StringComparer.OrdinalIgnoreCase);

        var list = SeparationList.Create(request.FileName);
        context.SeparationLists.Add(list);
        await context.SaveChangesAsync(cancellationToken);

        var items = new List<SeparationItem>();
        foreach (var p in parsed.Items)
        {
            var resolved = ResolveSkuParts(p.Sku, skuLookup);

            var resolvedColor = !string.IsNullOrWhiteSpace(p.Color) ? p.Color : (resolved.Color ?? "");
            var resolvedSize  = !string.IsNullOrWhiteSpace(p.Size)  ? p.Size  : (resolved.Size  ?? "");

            if (string.IsNullOrWhiteSpace(resolvedColor)) resolvedColor = "?";
            if (string.IsNullOrWhiteSpace(resolvedSize))  resolvedSize  = "?";

            var item = SeparationItem.Create(
                list.Id, p.Sku, resolved.Estampa ?? "", resolvedColor, resolvedSize, p.Quantity, p.SortOrder);

            if (resolved.DtfModelId.HasValue)
                item.SetDtfModel(resolved.DtfModelId);

            items.Add(item);
        }

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Estampa, i.Color, i.Size, i.Quantity, i.SortOrder, i.DtfModelId)).ToList()
        );
    }

    /// <summary>
    /// Parses a SKU. Dois formatos suportados:
    /// - 4+ segmentos: MODELAGEM-ESTAMPA-COR-TAMANHO (ex: REG-MADT-RED-G).
    ///   Posição 1 → Estampa (lookup Category=Estampa, retorna também DtfModelId).
    ///   Posição 2 → Cor (lookup Category=Cor).
    ///   Posição 3 → Tamanho (lookup Category=Tamanho).
    /// - Exatamente 3 segmentos (formato legado): MODELO-COR-TAMANHO (ex: BBL-BLK-M).
    ///   Posição 1 → Cor. Posição 2 → Tamanho. Sem Estampa.
    ///
    /// Posição 0 (Modelagem/Modelo) não é resolvida aqui — usada apenas como ModelCode
    /// bruto em outras partes do fluxo (checagem/dedução de estoque de camiseta).
    /// Segmentos extras (5º em diante) são ignorados.
    /// Se um segmento não tiver mapeamento configurado, o código bruto é usado como fallback.
    /// </summary>
    private static (string? Color, string? Size, string? Estampa, Guid? DtfModelId) ResolveSkuParts(
        string sku,
        ILookup<string, SkuCode> skuLookup)
    {
        if (string.IsNullOrWhiteSpace(sku)) return (null, null, null, null);

        var parts = sku.Split('-', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 4)
        {
            var estampaPart = parts[1].ToUpper();
            var estampaMatch = skuLookup[estampaPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Estampa);
            var estampa = estampaMatch?.Value ?? estampaPart;
            var dtfModelId = estampaMatch?.DtfModelId;

            var cor = ResolveSegment(parts[2], SkuCodeCategory.Cor, skuLookup);
            var tamanho = ResolveSegment(parts[3], SkuCodeCategory.Tamanho, skuLookup);

            return (cor, tamanho, estampa, dtfModelId);
        }

        if (parts.Length == 3)
        {
            var cor = ResolveSegment(parts[1], SkuCodeCategory.Cor, skuLookup);
            var tamanho = ResolveSegment(parts[2], SkuCodeCategory.Tamanho, skuLookup);

            return (cor, tamanho, null, null);
        }

        return (null, null, null, null);
    }

    /// <summary>
    /// Resolve um segmento de SKU para o valor mapeado em SkuCode na categoria informada.
    /// Se não houver mapeamento configurado, retorna o próprio código (em maiúsculas) como fallback.
    /// </summary>
    private static string ResolveSegment(
        string rawSegment, SkuCodeCategory category, ILookup<string, SkuCode> skuLookup)
    {
        var part = rawSegment.ToUpper();
        var match = skuLookup[part].FirstOrDefault(c => c.Category == category);
        return match?.Value ?? part;
    }
}
