using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfOrderConfiguration : IEntityTypeConfiguration<DtfOrder>
{
    public void Configure(EntityTypeBuilder<DtfOrder> builder)
    {
        builder.ToTable("DtfOrders");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.OrderNumber).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);

        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasMany(o => o.Items)
               .WithOne()
               .HasForeignKey(i => i.DtfOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Items)
               .HasField("_items")
               .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
