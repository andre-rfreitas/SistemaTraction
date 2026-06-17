using Microsoft.EntityFrameworkCore;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Separation;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<FabricType> FabricTypes { get; }
    DbSet<FabricColor> FabricColors { get; }
    DbSet<FabricRoll> FabricRolls { get; }
    DbSet<CuttingOrder> CuttingOrders { get; }
    DbSet<CuttingOrderItem> CuttingOrderItems { get; }
    DbSet<CuttingDelivery> CuttingDeliveries { get; }
    DbSet<SewingDelivery> SewingDeliveries { get; }
    DbSet<StockItem> StockItems { get; }
    DbSet<ShirtStockMovement> ShirtStockMovements { get; }
    DbSet<SeparationList> SeparationLists { get; }
    DbSet<SeparationItem> SeparationItems { get; }
    DbSet<SkuCode> SkuCodes { get; }
    DbSet<DtfModel> DtfModels { get; }
    DbSet<DtfStockItem> DtfStockItems { get; }
    DbSet<DtfStockMovement> DtfStockMovements { get; }
    DbSet<AppConfig> AppConfigs { get; }
    DbSet<FinancialEntry> FinancialEntries { get; }
    DbSet<SupplyType> SupplyTypes { get; }
    DbSet<SupplyStockItem> SupplyStockItems { get; }
    DbSet<SupplyStockMovement> SupplyStockMovements { get; }
    DbSet<SupplyOrderConfig> SupplyOrderConfigs { get; }
    DbSet<Sewer> Sewers { get; }
    DbSet<SewerProductType> SewerProductTypes { get; }
    DbSet<DtfOrder> DtfOrders { get; }
    DbSet<DtfOrderItem> DtfOrderItems { get; }
    DbSet<GenericProductCategory> GenericProductCategories { get; }
    DbSet<GenericProduct> GenericProducts { get; }
    DbSet<GenericProductMovement> GenericProductMovements { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
