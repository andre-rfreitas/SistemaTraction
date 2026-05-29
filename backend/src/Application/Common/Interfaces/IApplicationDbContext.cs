using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }
    DbSet<DtfModel> DtfModels { get; }
    DbSet<DtfStockItem> DtfStockItems { get; }
    DbSet<DtfStockMovement> DtfStockMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
