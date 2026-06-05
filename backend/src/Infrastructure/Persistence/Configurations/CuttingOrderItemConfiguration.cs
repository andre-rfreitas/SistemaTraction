using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class CuttingOrderItemConfiguration : IEntityTypeConfiguration<CuttingOrderItem>
{
    public void Configure(EntityTypeBuilder<CuttingOrderItem> builder)
    {
        builder.ToTable("CuttingOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.RequestedPiecesJson).IsRequired().HasColumnType("nvarchar(max)");

        builder.HasOne(i => i.FabricRoll)
            .WithMany()
            .HasForeignKey(i => i.FabricRollId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<CuttingOrder>()
            .WithMany(o => o.Items)
            .HasForeignKey(i => i.CuttingOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
