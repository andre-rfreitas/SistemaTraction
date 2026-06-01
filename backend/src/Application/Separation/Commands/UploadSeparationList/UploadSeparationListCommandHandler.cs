using MediatR;
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

        var list = SeparationList.Create(request.FileName);
        context.SeparationLists.Add(list);
        await context.SaveChangesAsync(cancellationToken);

        var items = parsed.Items
            .Select(p => SeparationItem.Create(list.Id, p.Sku, p.Color, p.Size, p.Quantity, p.SortOrder))
            .ToList();

        context.SeparationItems.AddRange(items);
        await context.SaveChangesAsync(cancellationToken);

        return new SeparationListDetailDto(
            list.Id,
            list.FileName,
            list.UploadedAt,
            list.Status.ToString(),
            items.Select(MapItem).ToList()
        );
    }

    private static SeparationItemDto MapItem(SeparationItem i) =>
        new(i.Id, i.Sku, i.Color, i.Size, i.Quantity, i.DtfModelId, null, i.SortOrder);
}
