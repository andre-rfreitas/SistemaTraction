using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Dtf;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class DtfModelConfiguration : IEntityTypeConfiguration<DtfModel>
{
    public void Configure(EntityTypeBuilder<DtfModel> builder)
    {
        builder.ToTable("DtfModels");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name).IsRequired().HasMaxLength(100);
        builder.Property(m => m.SheetLabel).IsRequired().HasMaxLength(50);
        builder.Property(m => m.StampsPerSheet).IsRequired();
        builder.Property(m => m.SheetCost).HasPrecision(18, 2);

        builder.HasIndex(m => m.Name).IsUnique();

        // Seed dos 6 modelos iniciais conforme especificação
        builder.HasData(
            new { Id = new Guid("11111111-0001-0000-0000-000000000000"), Name = "Angel",            SheetLabel = "Folha 1", StampsPerSheet = 9,  SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("11111111-0002-0000-0000-000000000000"), Name = "Flying Souls",     SheetLabel = "Folha 2", StampsPerSheet = 6,  SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("11111111-0003-0000-0000-000000000000"), Name = "Old Skull Tongue",  SheetLabel = "Folha 3", StampsPerSheet = 21, SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("11111111-0005-0000-0000-000000000000"), Name = "Red Rage",          SheetLabel = "Folha 5", StampsPerSheet = 4,  SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("11111111-0007-0000-0000-000000000000"), Name = "Flaming Skull",     SheetLabel = "Folha 7", StampsPerSheet = 8,  SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false },
            new { Id = new Guid("11111111-0009-0000-0000-000000000000"), Name = "Made in Traction",  SheetLabel = "Folha 9", StampsPerSheet = 5,  SheetCost = 49.80m, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), UpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), IsDeleted = false }
        );
    }
}
