using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FabricRollConfiguration : IEntityTypeConfiguration<FabricRoll>
{
    public void Configure(EntityTypeBuilder<FabricRoll> builder)
    {
        builder.ToTable("FabricRolls");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.WeightKg).HasPrecision(18, 3).IsRequired();
        builder.Property(r => r.PriceTotal).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.PricePerKgActual).HasPrecision(18, 4).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().IsRequired();

        builder.HasOne(r => r.FabricType)
            .WithMany()
            .HasForeignKey(r => r.FabricTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.FabricColor)
            .WithMany()
            .HasForeignKey(r => r.FabricColorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
