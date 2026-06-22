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

        // Load SKU code mappings (Modelo, Cor, Tamanho)
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
            var (color, size) = ResolveSkuParts(p.Sku, skuLookup);

            var resolvedColor = !string.IsNullOrWhiteSpace(p.Color) ? p.Color : (color ?? "");
            var resolvedSize  = !string.IsNullOrWhiteSpace(p.Size)  ? p.Size  : (size  ?? "");

            if (string.IsNullOrWhiteSpace(resolvedColor)) resolvedColor = "?";
            if (string.IsNullOrWhiteSpace(resolvedSize))  resolvedSize  = "?";

            var item = SeparationItem.Create(list.Id, p.Sku, resolvedColor, resolvedSize, p.Quantity, p.SortOrder);
            items.Add(item);
        }

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Color, i.Size, i.Quantity, i.SortOrder)).ToList()
        );
    }

    /// <summary>
    /// Parses a SKU in the format MODELO-COR-TAMANHO (e.g. BBL-BLK-M).
    /// - Position 0: Modelo  — looked up but only stored as SKU string; not used for stock.
    /// - Position 1: Cor     — resolved via SkuCode lookup (Category = Cor).
    /// - Position 2: Tamanho — resolved via SkuCode lookup (Category = Tamanho).
    ///
    /// Extra segments are ignored so the parser remains extensible.
    /// If a segment has no mapping the raw code is used as fallback.
    /// </summary>
    private static (string? color, string? size) ResolveSkuParts(
        string sku,
        ILookup<string, Domain.Separation.SkuCode> skuLookup)
    {
        if (string.IsNullOrWhiteSpace(sku)) return (null, null);

        var parts = sku.Split('-', StringSplitOptions.RemoveEmptyEntries);

        // Segment 1 → Cor
        string? color = null;
        if (parts.Length > 1)
        {
            var part = parts[1].ToUpper();
            var match = skuLookup[part].FirstOrDefault(c => c.Category == SkuCodeCategory.Cor);
            color = match?.Value ?? part; // fallback: use the raw code
        }

        // Segment 2 → Tamanho
        string? size = null;
        if (parts.Length > 2)
        {
            var part = parts[2].ToUpper();
            var match = skuLookup[part].FirstOrDefault(c => c.Category == SkuCodeCategory.Tamanho);
            size = match?.Value ?? part; // fallback: use the raw code
        }

        return (color, size);
    }
}
