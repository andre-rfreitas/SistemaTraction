using Microsoft.EntityFrameworkCore;
using SistemaTraction.Application.Common.Interfaces;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Application.Tests;

public class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options) : base(options) { }

    public DbSet<FabricType> FabricTypes => Set<FabricType>();
    public DbSet<FabricColor> FabricColors => Set<FabricColor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FabricType>(b =>
        {
            b.HasKey(t => t.Id);
            b.HasMany(t => t.Colors)
             .WithOne(c => c.FabricType)
             .HasForeignKey(c => c.FabricTypeId);
        });

        modelBuilder.Entity<FabricColor>(b =>
        {
            b.HasKey(c => c.Id);
        });
    }
}
