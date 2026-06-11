using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Sewing;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SewerConfiguration : IEntityTypeConfiguration<Sewer>
{
    public void Configure(EntityTypeBuilder<Sewer> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Phone)
            .HasMaxLength(30);

        builder.HasMany(s => s.ProductTypes)
            .WithOne(pt => pt.Sewer)
            .HasForeignKey(pt => pt.SewerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}
