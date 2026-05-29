using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();
    public DbSet<FabricRoll> FabricRolls => Set<FabricRoll>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<CuttingDelivery> CuttingDeliveries => Set<CuttingDelivery>();
    public DbSet<DtfModel> DtfModels => Set<DtfModel>();
    public DbSet<DtfStockItem> DtfStockItems => Set<DtfStockItem>();
    public DbSet<DtfStockMovement> DtfStockMovements => Set<DtfStockMovement>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();
    public DbSet<FinancialEntry> FinancialEntries => Set<FinancialEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
