using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class GenericProductMovementConfiguration : IEntityTypeConfiguration<GenericProductMovement>
{
    public void Configure(EntityTypeBuilder<GenericProductMovement> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ProductName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.Reason).HasMaxLength(500).IsRequired();
        builder.Property(m => m.Origin).HasMaxLength(50).IsRequired();

        builder.HasOne(m => m.GenericProduct)
               .WithMany()
               .HasForeignKey(m => m.GenericProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
