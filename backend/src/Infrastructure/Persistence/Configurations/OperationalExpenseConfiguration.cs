using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Financial;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class OperationalExpenseConfiguration : IEntityTypeConfiguration<OperationalExpense>
{
    public void Configure(EntityTypeBuilder<OperationalExpense> builder)
    {
        builder.ToTable("OperationalExpenses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FixedMonthlyValue).HasColumnType("decimal(18,2)");
        builder.Property(e => e.RatePercent).HasColumnType("decimal(10,4)");
    }
}
