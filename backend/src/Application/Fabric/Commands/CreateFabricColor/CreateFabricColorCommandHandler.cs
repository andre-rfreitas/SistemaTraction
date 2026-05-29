using MediatR;
using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Common;

namespace SistemaTraction.Application.Fabric.Commands.CreateFabricColor;

public class CreateFabricColorCommandHandler(IApplicationDbContext context)
    : IRequestHandler<CreateFabricColorCommand, Guid>
{
    public async Task<Guid> Handle(CreateFabricColorCommand request, CancellationToken cancellationToken)
    {
        var fabricType = await context.FabricTypes
            .Include(t => t.Colors)
            .FirstOrDefaultAsync(t => t.Id == request.FabricTypeId && !t.IsDeleted, cancellationToken)
            ?? throw new DomainException("Tipo de tecido não encontrado.");

        var color = fabricType.AddColor(request.Name, request.HexCode);

        // Registrar explicitamente no change tracker (EF Core InMemory não detecta
        // entidades novas adicionadas via métodos de domínio em coleções backed field)
        context.FabricColors.Add(color);

        await context.SaveChangesAsync(cancellationToken);

        return color.Id;
    }
}
