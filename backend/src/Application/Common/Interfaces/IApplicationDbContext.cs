using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }
    DbSet<FabricRoll> FabricRolls { get; }
    DbSet<CuttingOrder> CuttingOrders { get; }
    DbSet<CuttingDelivery> CuttingDeliveries { get; }
    DbSet<SewingDelivery> SewingDeliveries { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<DtfModel> DtfModels { get; }
    DbSet<DtfStockItem> DtfStockItems { get; }
    DbSet<DtfStockMovement> DtfStockMovements { get; }
    DbSet<AppConfig> AppConfigs { get; }
    DbSet<FinancialEntry> FinancialEntries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
