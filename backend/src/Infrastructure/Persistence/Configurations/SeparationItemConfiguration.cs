using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Separation;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SeparationItemConfiguration : IEntityTypeConfiguration<SeparationItem>
{
    public void Configure(EntityTypeBuilder<SeparationItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Sku).HasMaxLength(100);
        builder.Property(i => i.Estampa).HasMaxLength(100);
        builder.Property(i => i.Color).HasMaxLength(100).IsRequired();
        builder.Property(i => i.Size).HasMaxLength(10).IsRequired();

        // Relationship to SeparationList is configured from the SeparationList side

        builder.HasOne(i => i.DtfModel)
            .WithMany()
            .HasForeignKey(i => i.DtfModelId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.SeparationListId);
    }
}
