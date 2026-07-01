using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Sewing;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SewingDeliveryConfiguration : IEntityTypeConfiguration<SewingDelivery>
{
    public void Configure(EntityTypeBuilder<SewingDelivery> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.GoodPiecesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(s => s.DefectivePiecesJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(s => s.GoodPiecesByItemJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(s => s.DefectivePiecesByItemJson)
            .HasColumnType("nvarchar(max)")
            .HasDefaultValue("{}")
            .IsRequired();

        builder.Property(s => s.SewingCostTotal)
            .HasPrecision(18, 4);

        builder.Property(s => s.DefectCostTotal)
            .HasPrecision(18, 4);

        builder.Property(s => s.IsPartial)
            .HasDefaultValue(false);

        builder.HasIndex(s => s.CuttingOrderId);

        builder.HasOne(s => s.CuttingOrder)
            .WithMany()
            .HasForeignKey(s => s.CuttingOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
