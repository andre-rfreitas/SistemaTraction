using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Cutting;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class CuttingOrderConfiguration : IEntityTypeConfiguration<CuttingOrder>
{
    public void Configure(EntityTypeBuilder<CuttingOrder> builder)
    {
        builder.ToTable("CuttingOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired();
        builder.Property(o => o.RequestedPiecesJson).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(o => o.Status).HasConversion<string>().IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(500);

        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasOne(o => o.FabricRoll)
            .WithMany()
            .HasForeignKey(o => o.FabricRollId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
