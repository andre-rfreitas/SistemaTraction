using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Stock;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class GenericProductConfiguration : IEntityTypeConfiguration<GenericProduct>
{
    public void Configure(EntityTypeBuilder<GenericProduct> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        
        builder.HasOne(p => p.Category)
               .WithMany()
               .HasForeignKey(p => p.CategoryId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.CategoryId, p.Name }).IsUnique();
    }
}
