using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
