using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SupplyOrderConfigConfiguration : IEntityTypeConfiguration<SupplyOrderConfig>
{
    public void Configure(EntityTypeBuilder<SupplyOrderConfig> builder)
    {
        builder.ToTable("SupplyOrderConfigs");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.QuantityPerOrder).IsRequired();

        builder.HasOne(c => c.SupplyType)
               .WithOne()
               .HasForeignKey<SupplyOrderConfig>(c => c.SupplyTypeId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.SupplyTypeId).IsUnique();
    }
}
