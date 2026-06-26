using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SkuCodeConfiguration : IEntityTypeConfiguration<SkuCode>
{
    public void Configure(EntityTypeBuilder<SkuCode> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasMaxLength(20).IsRequired();
        builder.Property(c => c.Value).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(20);

        builder.HasOne<DtfModel>()
            .WithMany()
            .HasForeignKey(c => c.DtfModelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
