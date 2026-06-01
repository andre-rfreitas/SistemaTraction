using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();
    public DbSet<FabricRoll> FabricRolls => Set<FabricRoll>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<CuttingDelivery> CuttingDeliveries => Set<CuttingDelivery>();
    public DbSet<SewingDelivery> SewingDeliveries => Set<SewingDelivery>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<DtfModel> DtfModels => Set<DtfModel>();
    public DbSet<DtfStockItem> DtfStockItems => Set<DtfStockItem>();
    public DbSet<DtfStockMovement> DtfStockMovements => Set<DtfStockMovement>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();
    public DbSet<FinancialEntry> FinancialEntries => Set<FinancialEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FabricType>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasMany(t => t.Colors)
             .WithOne(c => c.FabricType)
             .HasForeignKey(c => c.FabricTypeId);
            b.Navigation(t => t.Colors)
             .HasField("_colors")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<FabricColor>(b =>
        {
            b.HasKey(c => c.Id);
        });

        modelBuilder.Entity<FabricRoll>(b =>
        {
            b.HasKey(r => r.Id);
            b.Property(r => r.Status).HasConversion<string>();
            b.HasOne(r => r.FabricType).WithMany().HasForeignKey(r => r.FabricTypeId);
            b.HasOne(r => r.FabricColor).WithMany().HasForeignKey(r => r.FabricColorId);
        });

        modelBuilder.Entity<CuttingOrder>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Status).HasConversion<string>();
            b.HasOne(o => o.FabricRoll).WithMany().HasForeignKey(o => o.FabricRollId);
        });

        modelBuilder.Entity<CuttingDelivery>(b =>
        {
            b.HasKey(d => d.Id);
            b.HasOne(d => d.CuttingOrder).WithMany().HasForeignKey(d => d.CuttingOrderId);
        });

        modelBuilder.Entity<SewingDelivery>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasOne(s => s.CuttingOrder).WithMany().HasForeignKey(s => s.CuttingOrderId);
        });

        modelBuilder.Entity<StockItem>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasIndex(s => new { s.FabricColorId, s.Size }).IsUnique();
        });

        modelBuilder.Entity<DtfModel>(b =>
        {
            b.HasKey(m => m.Id);
        });

        modelBuilder.Entity<DtfStockItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.HasOne(i => i.DtfModel)
             .WithOne()
             .HasForeignKey<DtfStockItem>(i => i.DtfModelId);
            b.HasMany(i => i.Movements)
             .WithOne(m => m.DtfStockItem)
             .HasForeignKey(m => m.DtfStockItemId);
            b.Navigation(i => i.Movements)
             .HasField("_movements")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DtfStockMovement>(b =>
        {
            b.HasKey(m => m.Id);
        });

        modelBuilder.Entity<AppConfig>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasIndex(c => c.Key).IsUnique();
        });

        modelBuilder.Entity<FinancialEntry>(b =>
        {
            b.HasKey(e => e.Id);
            b.Property(e => e.Type).HasConversion<string>();
        });
    }
}
