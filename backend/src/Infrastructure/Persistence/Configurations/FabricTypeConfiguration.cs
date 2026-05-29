using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FabricTypeConfiguration : IEntityTypeConfiguration<FabricType>
{
    public void Configure(EntityTypeBuilder<FabricType> builder)
    {
        builder.ToTable("FabricTypes");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Variation).IsRequired().HasMaxLength(100);
        builder.Property(t => t.PricePerKg).HasPrecision(18, 2);
        builder.Property(t => t.AverageKgPerRoll).HasPrecision(18, 3);

        builder.HasIndex(t => new { t.Name, t.Variation }).IsUnique();

        builder.HasMany(t => t.Colors)
            .WithOne(c => c.FabricType)
            .HasForeignKey(c => c.FabricTypeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(t => t.Colors)
            .HasField("_colors")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
