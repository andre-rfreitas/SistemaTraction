using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SupplyStockItemConfiguration : IEntityTypeConfiguration<SupplyStockItem>
{
    public void Configure(EntityTypeBuilder<SupplyStockItem> builder)
    {
        builder.ToTable("SupplyStockItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Quantity).IsRequired();

        builder.HasOne(i => i.SupplyType)
               .WithOne()
               .HasForeignKey<SupplyStockItem>(i => i.SupplyTypeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Movements)
               .WithOne(m => m.SupplyStockItem)
               .HasForeignKey(m => m.SupplyStockItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Movements)
               .HasField("_movements")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => i.SupplyTypeId).IsUnique();
    }
}
