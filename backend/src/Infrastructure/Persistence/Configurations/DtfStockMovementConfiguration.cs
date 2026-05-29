using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfStockMovementConfiguration : IEntityTypeConfiguration<DtfStockMovement>
{
    public void Configure(EntityTypeBuilder<DtfStockMovement> builder)
    {
        builder.ToTable("DtfStockMovements");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Type).IsRequired();
        builder.Property(m => m.Delta).IsRequired();
        builder.Property(m => m.Reason).HasMaxLength(500);
    }
}
