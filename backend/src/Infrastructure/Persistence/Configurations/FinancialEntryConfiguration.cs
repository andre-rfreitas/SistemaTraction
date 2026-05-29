using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FinancialEntryConfiguration : IEntityTypeConfiguration<FinancialEntry>
{
    public void Configure(EntityTypeBuilder<FinancialEntry> builder)
    {
        builder.ToTable("FinancialEntries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).HasConversion<string>().IsRequired();
        builder.Property(e => e.Category).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Amount).HasPrecision(18, 2).IsRequired();
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.ReferenceType).HasMaxLength(100);

        builder.HasIndex(e => e.Category);
        builder.HasIndex(e => e.EntryDate);
    }
}
