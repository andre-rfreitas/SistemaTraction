using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FabricColorName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.FabricTypeName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.FabricTypeVariation).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Size).HasMaxLength(10).IsRequired();
        builder.Property(s => s.ShirtType).HasConversion<string>().HasMaxLength(20).IsRequired()
            .HasDefaultValue(SistemaTraction.Domain.Stock.ShirtType.Regular);

        builder.HasIndex(s => new { s.FabricColorId, s.Size, s.ShirtType }).IsUnique();
    }
}
