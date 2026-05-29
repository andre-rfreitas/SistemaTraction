using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();
    public DbSet<DtfModel> DtfModels => Set<DtfModel>();
    public DbSet<DtfStockItem> DtfStockItems => Set<DtfStockItem>();
    public DbSet<DtfStockMovement> DtfStockMovements => Set<DtfStockMovement>();

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
    }
}
