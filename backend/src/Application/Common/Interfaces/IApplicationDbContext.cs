using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }
    DbSet<FabricRoll> FabricRolls { get; }
    DbSet<CuttingOrder> CuttingOrders { get; }
    DbSet<DtfModel> DtfModels { get; }
    DbSet<DtfStockItem> DtfStockItems { get; }
    DbSet<DtfStockMovement> DtfStockMovements { get; }
    DbSet<AppConfig> AppConfigs { get; }
    DbSet<FinancialEntry> FinancialEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
