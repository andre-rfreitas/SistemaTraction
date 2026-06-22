using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class ShirtStockMovementConfiguration : IEntityTypeConfiguration<ShirtStockMovement>
{
    public void Configure(EntityTypeBuilder<ShirtStockMovement> builder)
    {
        builder.ToTable("ShirtStockMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FabricColorName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.Size).IsRequired().HasMaxLength(10);
        builder.Property(m => m.ModelCode).HasMaxLength(50).IsRequired()
            .HasDefaultValue("REG");
        builder.Property(m => m.Reason).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Origin).IsRequired().HasMaxLength(50);

        builder.HasIndex(m => m.FabricColorId);
        builder.HasIndex(m => m.CreatedAt);

        builder.HasOne(m => m.StockItem)
            .WithMany()
            .HasForeignKey(m => m.StockItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
