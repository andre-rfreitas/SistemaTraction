using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        // DtfModelId is a bare FK — no navigation property needed
        builder.Property(c => c.DtfModelId).IsRequired(false);
    }
}
