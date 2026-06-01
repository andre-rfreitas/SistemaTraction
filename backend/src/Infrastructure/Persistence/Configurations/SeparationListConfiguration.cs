using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SeparationListConfiguration : IEntityTypeConfiguration<SeparationList>
{
    public void Configure(EntityTypeBuilder<SeparationList> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.FileName).HasMaxLength(500).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(l => l.Items)
            .WithOne(i => i.SeparationList)
            .HasForeignKey(i => i.SeparationListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(l => l.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
