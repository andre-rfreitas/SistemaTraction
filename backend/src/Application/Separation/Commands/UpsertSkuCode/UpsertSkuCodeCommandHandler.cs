using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Application.Separation.DTOs;
using SistemaTraction.Domain.Common;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Application.Separation.Commands.UpsertSkuCode;

public class UpsertSkuCodeCommandHandler(IApplicationDbContext context)
    : IRequestHandler<UpsertSkuCodeCommand, SkuCodeDto>
{
    public async Task<SkuCodeDto> Handle(UpsertSkuCodeCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<SkuCodeCategory>(request.Category, out var category))
            throw new DomainException($"Categoria inválida: {request.Category}. Use: Modelo, Cor, Tamanho.");

        SkuCode skuCode;

        if (request.Id.HasValue)
        {
            skuCode = await context.SkuCodes
                .FirstOrDefaultAsync(c => c.Id == request.Id.Value && !c.IsDeleted, cancellationToken)
                ?? throw new DomainException("Código SKU não encontrado.");

            skuCode.Update(request.Code, request.Value, category);
        }
        else
        {
            skuCode = SkuCode.Create(request.Code, request.Value, category);
            context.SkuCodes.Add(skuCode);
        }

        await context.SaveChangesAsync(cancellationToken);

        return new SkuCodeDto(skuCode.Id, skuCode.Code, skuCode.Value, skuCode.Category.ToString());
    }
}
