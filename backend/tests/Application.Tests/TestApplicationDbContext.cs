using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Config;
using SistemaTraction.Domain.Cutting;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;
using SistemaTraction.Domain.Financial;
using SistemaTraction.Domain.Separation;
using SistemaTraction.Domain.Sewing;
using SistemaTraction.Domain.Stock;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();
    public DbSet<FabricRoll> FabricRolls => Set<FabricRoll>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<CuttingOrderItem> CuttingOrderItems => Set<CuttingOrderItem>();
    public DbSet<CuttingDelivery> CuttingDeliveries => Set<CuttingDelivery>();
    public DbSet<SewingDelivery> SewingDeliveries => Set<SewingDelivery>();
    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<ShirtStockMovement> ShirtStockMovements => Set<ShirtStockMovement>();
    public DbSet<SeparationList> SeparationLists => Set<SeparationList>();
    public DbSet<SeparationItem> SeparationItems => Set<SeparationItem>();
    public DbSet<SkuCode> SkuCodes => Set<SkuCode>();
    public DbSet<DtfModel> DtfModels => Set<DtfModel>();
    public DbSet<DtfStockItem> DtfStockItems => Set<DtfStockItem>();
    public DbSet<DtfStockMovement> DtfStockMovements => Set<DtfStockMovement>();
    public DbSet<AppConfig> AppConfigs => Set<AppConfig>();
    public DbSet<FinancialEntry> FinancialEntries => Set<FinancialEntry>();
    public DbSet<SupplyType> SupplyTypes => Set<SupplyType>();
    public DbSet<SupplyStockItem> SupplyStockItems => Set<SupplyStockItem>();
    public DbSet<SupplyStockMovement> SupplyStockMovements => Set<SupplyStockMovement>();
    public DbSet<SupplyOrderConfig> SupplyOrderConfigs => Set<SupplyOrderConfig>();
    public DbSet<Sewer> Sewers => Set<Sewer>();
    public DbSet<SewerProductType> SewerProductTypes => Set<SewerProductType>();
    public DbSet<DtfOrder> DtfOrders => Set<DtfOrder>();
    public DbSet<DtfOrderItem> DtfOrderItems => Set<DtfOrderItem>();

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
        });

        modelBuilder.Entity<CuttingOrder>(b =>
        {
            b.Navigation(o => o.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<CuttingOrderItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.HasOne(i => i.FabricRoll).WithMany().HasForeignKey(i => i.FabricRollId);
            b.HasOne<CuttingOrder>().WithMany(o => o.Items).HasForeignKey(i => i.CuttingOrderId);
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

        modelBuilder.Entity<ShirtStockMovement>(b =>
        {
            b.HasKey(m => m.Id);
            b.HasOne(m => m.StockItem).WithMany().HasForeignKey(m => m.StockItemId).IsRequired(false);
        });

        modelBuilder.Entity<SeparationList>(b =>
        {
            b.HasKey(l => l.Id);
            b.Property(l => l.Status).HasConversion<string>();
            b.HasMany(l => l.Items)
             .WithOne(i => i.SeparationList)
             .HasForeignKey(i => i.SeparationListId);
            b.Navigation(l => l.Items)
             .HasField("_items")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SeparationItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.HasOne(i => i.DtfModel).WithMany().HasForeignKey(i => i.DtfModelId).IsRequired(false);
        });

        modelBuilder.Entity<SkuCode>(b =>
        {
            b.HasKey(c => c.Id);
            b.Property(c => c.Category).HasConversion<string>();
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

        modelBuilder.Entity<SupplyType>(b =>
        {
            b.HasKey(t => t.Id);
            b.Property(t => t.YieldBasis).HasConversion<string>();
        });

        modelBuilder.Entity<SupplyStockItem>(b =>
        {
            b.HasKey(i => i.Id);
            b.HasOne(i => i.SupplyType).WithOne().HasForeignKey<SupplyStockItem>(i => i.SupplyTypeId);
            b.HasMany(i => i.Movements)
             .WithOne(m => m.SupplyStockItem)
             .HasForeignKey(m => m.SupplyStockItemId);
            b.Navigation(i => i.Movements)
             .HasField("_movements")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SupplyStockMovement>(b =>
        {
            b.HasKey(m => m.Id);
            b.Property(m => m.Type).HasConversion<string>();
        });

        modelBuilder.Entity<SupplyOrderConfig>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasOne(c => c.SupplyType).WithOne().HasForeignKey<SupplyOrderConfig>(c => c.SupplyTypeId);
            b.HasIndex(c => c.SupplyTypeId).IsUnique();
        });

        modelBuilder.Entity<Sewer>(b =>
        {
            b.HasKey(s => s.Id);
            b.HasMany(s => s.ProductTypes)
             .WithOne(pt => pt.Sewer)
             .HasForeignKey(pt => pt.SewerId)
             .OnDelete(DeleteBehavior.Cascade);
            b.Navigation(s => s.ProductTypes)
             .HasField("_productTypes")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<SewerProductType>(b => b.HasKey(pt => pt.Id));

        modelBuilder.Entity<DtfOrder>(b =>
        {
            b.HasKey(o => o.Id);
            b.Property(o => o.Status).HasConversion<string>();
            b.HasMany(o => o.Items)
             .WithOne()
             .HasForeignKey(i => i.DtfOrderId);
            b.Navigation(o => o.Items)
             .HasField("_items")
             .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<DtfOrderItem>(b =>
        {
            b.HasKey(i => i.Id);
        });
    }
}
