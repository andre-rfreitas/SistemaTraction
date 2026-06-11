using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Sewing;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class SewerProductTypeConfiguration : IEntityTypeConfiguration<SewerProductType>
{
    public void Configure(EntityTypeBuilder<SewerProductType> builder)
    {
        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(pt => pt.PriceDefault)
            .HasPrecision(18, 4);

        builder.Property(pt => pt.PriceG1)
            .HasPrecision(18, 4);

        builder.HasQueryFilter(pt => !pt.IsDeleted);
    }
}
