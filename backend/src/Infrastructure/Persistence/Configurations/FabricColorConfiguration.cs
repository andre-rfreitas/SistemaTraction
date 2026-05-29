using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Fabric;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class FabricColorConfiguration : IEntityTypeConfiguration<FabricColor>
{
    public void Configure(EntityTypeBuilder<FabricColor> builder)
    {
        builder.ToTable("FabricColors");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.HexCode).HasMaxLength(7);
    }
}
