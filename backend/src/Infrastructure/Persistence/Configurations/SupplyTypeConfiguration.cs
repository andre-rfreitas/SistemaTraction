using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Supplies;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SupplyTypeConfiguration : IEntityTypeConfiguration<SupplyType>
{
    public void Configure(EntityTypeBuilder<SupplyType> builder)
    {
        builder.ToTable("SupplyTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Unit).IsRequired().HasMaxLength(20);
        builder.Property(t => t.PricePerUnit).HasPrecision(18, 4);
        builder.Property(t => t.YieldBasis)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(YieldBasis.None);
        builder.Property(t => t.YieldQuantity).HasPrecision(18, 4);
        builder.Property(t => t.YieldProductName).HasMaxLength(100);
    }
}
