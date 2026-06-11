using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SupplyStockMovementConfiguration : IEntityTypeConfiguration<SupplyStockMovement>
{
    public void Configure(EntityTypeBuilder<SupplyStockMovement> builder)
    {
        builder.ToTable("SupplyStockMovements");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Type).HasConversion<string>().IsRequired();
        builder.Property(m => m.Delta).IsRequired();
        builder.Property(m => m.Reason).HasMaxLength(200);
        builder.Property(m => m.SupplierName).HasMaxLength(200);
        builder.Property(m => m.SupplierPhone).HasMaxLength(30);
        builder.Property(m => m.UnitPrice).HasPrecision(18, 4);
        builder.Property(m => m.TotalCost).HasPrecision(18, 4);
    }
}
