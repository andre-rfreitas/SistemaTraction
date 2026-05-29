using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class CuttingDeliveryConfiguration : IEntityTypeConfiguration<CuttingDelivery>
{
    public void Configure(EntityTypeBuilder<CuttingDelivery> builder)
    {
        builder.ToTable("CuttingDeliveries");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeliveredPiecesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(d => d.CuttingCostTotal).HasPrecision(18, 2).IsRequired();

        builder.HasOne(d => d.CuttingOrder)
            .WithMany()
            .HasForeignKey(d => d.CuttingOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.CuttingOrderId).IsUnique();
    }
}
