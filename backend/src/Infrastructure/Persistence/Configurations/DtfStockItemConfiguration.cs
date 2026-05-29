using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfStockItemConfiguration : IEntityTypeConfiguration<DtfStockItem>
{
    public void Configure(EntityTypeBuilder<DtfStockItem> builder)
    {
        builder.ToTable("DtfStockItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.CurrentQuantity).IsRequired();

        builder.HasOne(i => i.DtfModel)
               .WithOne()
               .HasForeignKey<DtfStockItem>(i => i.DtfModelId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Movements)
               .WithOne(m => m.DtfStockItem)
               .HasForeignKey(m => m.DtfStockItemId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(i => i.Movements)
               .HasField("_movements")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(i => i.DtfModelId).IsUnique();
    }
}
