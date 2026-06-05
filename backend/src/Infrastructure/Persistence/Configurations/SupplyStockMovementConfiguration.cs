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
    }
}
