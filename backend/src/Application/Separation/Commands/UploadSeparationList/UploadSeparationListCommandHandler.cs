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

        var skuCodes = await context.SkuCodes
            .Where(c => !c.IsDeleted)
            .ToListAsync(cancellationToken);

        var dtfModels = await context.DtfModels
            .Where(m => !m.IsDeleted)
            .ToDictionaryAsync(m => m.Id, m => m.Name, cancellationToken);

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
            var (color, size, dtfModelId) = ResolveSkuParts(p.Sku, skuLookup, tamanhoCodes);

            var resolvedColor = !string.IsNullOrWhiteSpace(p.Color) ? p.Color : (color ?? "");
            var resolvedSize  = !string.IsNullOrWhiteSpace(p.Size)  ? p.Size  : (size  ?? "");

            if (string.IsNullOrWhiteSpace(resolvedColor)) resolvedColor = "?";
            if (string.IsNullOrWhiteSpace(resolvedSize))  resolvedSize  = "?";

            var item = SeparationItem.Create(list.Id, p.Sku, resolvedColor, resolvedSize, p.Quantity, p.SortOrder);
            if (dtfModelId.HasValue) item.SetDtfModel(dtfModelId);
            items.Add(item);
        }

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id, list.FileName, list.UploadedAt, list.Status.ToString(),
            items.Select(i => new SeparationItemDto(
                i.Id, i.Sku, i.Color, i.Size, i.Quantity,
                i.DtfModelId,
                i.DtfModelId.HasValue ? dtfModels.GetValueOrDefault(i.DtfModelId.Value) : null,
                i.SortOrder)).ToList()
        );
    }

    private static (string? color, string? size, Guid? dtfModelId) ResolveSkuParts(
        string sku,
        ILookup<string, Domain.Separation.SkuCode> skuLookup,
        List<string> tamanhoCodes)
    {
        string? color = null, size = null;
        Guid? dtfModelId = null;

        if (string.IsNullOrWhiteSpace(sku)) return (null, null, null);

        foreach (var raw in sku.Split('-', StringSplitOptions.RemoveEmptyEntries))
        {
            var part = raw.ToUpper();
            var match = skuLookup[part].FirstOrDefault();

            if (match != null)
            {
                switch (match.Category)
                {
                    case SkuCodeCategory.Cor:        color      = match.Value; break;
                    case SkuCodeCategory.Tamanho:    size       = match.Value; break;
                    case SkuCodeCategory.EstampaDtf: dtfModelId = match.DtfModelId; break;
                    // Modelo: ignored for now
                }
            }
            else
            {
                // Try to split concatenated color+size (e.g., REDGG → RED + GG, BLKG1 → BLK + G1)
                foreach (var sizeCode in tamanhoCodes)
                {
                    if (part.Length > sizeCode.Length && part.EndsWith(sizeCode, StringComparison.OrdinalIgnoreCase))
                    {
                        var colorPart  = part[..^sizeCode.Length];
                        var colorMatch = skuLookup[colorPart].FirstOrDefault(c => c.Category == SkuCodeCategory.Cor);
                        var sizeMatch  = skuLookup[sizeCode].FirstOrDefault(c => c.Category == SkuCodeCategory.Tamanho);

                        if (colorMatch != null) color = colorMatch.Value;
                        if (sizeMatch  != null) size  = sizeMatch.Value;
                        break;
                    }
                }
            }
        }

        return (color, size, dtfModelId);
    }
}
