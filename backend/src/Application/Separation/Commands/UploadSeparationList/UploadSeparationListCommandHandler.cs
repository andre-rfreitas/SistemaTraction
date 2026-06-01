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
                "Verifique se o arquivo é uma lista de separação válida do ERP.");

        // Load SKU codes and DTF models for resolution
        var skuCodes = await context.SkuCodes
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtfModels = await context.DtfModels
            .Where(m => !m.IsDeleted)
            .ToListAsync(cancellationToken);

        var skuLookup = skuCodes.ToLookup(c => c.Code, StringComparer.OrdinalIgnoreCase);
        var tamanhoCodes = skuCodes
            .Where(c => c.Category == SkuCodeCategory.Tamanho)
            .Select(c => c.Code)
            .OrderByDescending(c => c.Length)
            .ToList();

        var list = SeparationList.Create(request.FileName);
        context.SeparationLists.Add(list);
        await context.SaveChangesAsync(cancellationToken);

        var items = new List<SeparationItem>();
        foreach (var p in parsed.Items)
        {
            // Resolve SKU parts into color, size, dtfModelId
            var (color, size, dtfModelName) = ResolveSkuParts(p.Sku, skuLookup, tamanhoCodes);

            var resolvedColor = !string.IsNullOrWhiteSpace(p.Color) ? p.Color : (color ?? "");
            var resolvedSize  = !string.IsNullOrWhiteSpace(p.Size)  ? p.Size  : (size  ?? "");

            Guid? dtfModelId = null;
            if (!string.IsNullOrWhiteSpace(dtfModelName))
            {
                var dtf = dtfModels.FirstOrDefault(m =>
                    m.Name.Equals(dtfModelName, StringComparison.OrdinalIgnoreCase));
                dtfModelId = dtf?.Id;
            }

            if (string.IsNullOrWhiteSpace(resolvedSize))
                resolvedSize = "?";

            if (string.IsNullOrWhiteSpace(resolvedColor))
                resolvedColor = "?";

            items.Add(SeparationItem.Create(list.Id, p.Sku, resolvedColor, resolvedSize, p.Quantity, p.SortOrder));
            if (dtfModelId.HasValue) items[^1].SetDtfModel(dtfModelId);
        }

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        var dtfModelMap = dtfModels.ToDictionary(m => m.Id, m => m.Name);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Color, i.Size, i.Quantity,
                i.DtfModelId,
                i.DtfModelId.HasValue ? dtfModelMap.GetValueOrDefault(i.DtfModelId.Value) : null,
                i.SortOrder)).ToList()
        );
    }

    private static (string? color, string? size, string? dtfModel) ResolveSkuParts(
        string sku,
        ILookup<string, Domain.Separation.SkuCode> skuLookup,
        List<string> tamanhoCodes)
    {
        string? color = null, size = null, dtfModel = null;

        if (string.IsNullOrWhiteSpace(sku)) return (null, null, null);

        var parts = sku.Split('-', StringSplitOptions.RemoveEmptyEntries);

        foreach (var raw in parts)
        {
            var part = raw.ToUpper();
            var match = skuLookup[part].FirstOrDefault();

            if (match != null)
            {
                switch (match.Category)
                {
                    case SkuCodeCategory.Cor:        color    = match.Value; break;
                    case SkuCodeCategory.Tamanho:    size     = match.Value; break;
                    case SkuCodeCategory.EstampaDtf: dtfModel = match.Value; break;
                }
            }
            else
            {
                // Try to split concatenated color+size (e.g., REDGG → RED + GG, BLKG1 → BLK + G1)
                foreach (var sizeCode in tamanhoCodes)
                {
                    if (part.Length > sizeCode.Length && part.EndsWith(sizeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        var colorPart = part[..^sizeCode.Length];
                        var colorMatch = skuLookup[colorPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Cor);
                        var sizeMatch  = skuLookup[sizeCode].FirstOrDefault(c => c.Category == SkuCodeCategory.Tamanho);

                        if (colorMatch != null) color = colorMatch.Value;
                        if (sizeMatch  != null) size  = sizeMatch.Value;
                        break;
                    }
                }
            }
        }

        return (color, size, dtfModel);
    }
}
