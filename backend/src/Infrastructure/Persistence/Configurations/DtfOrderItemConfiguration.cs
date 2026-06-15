using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfOrderItemConfiguration : IEntityTypeConfiguration<DtfOrderItem>
{
    public void Configure(EntityTypeBuilder<DtfOrderItem> builder)
    {
        builder.ToTable("DtfOrderItems");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.SheetQuantity).IsRequired();

        builder.HasOne<DtfModel>()
               .WithMany()
               .HasForeignKey(i => i.DtfModelId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
