using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaTraction.Domain.Config;

namespace SistemaTraction.Infrastructure.Persistence.Configurations;

public class AppConfigConfiguration : IEntityTypeConfiguration<AppConfig>
{
    private static readonly DateTime SeedDate =
        new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public void Configure(EntityTypeBuilder<AppConfig> builder)
    {
        builder.ToTable("AppConfigs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Key).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Value).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.Description).HasMaxLength(500);

        builder.HasIndex(c => c.Key).IsUnique();

        builder.HasData(
            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110001"), "dtf.alerta_estoque_minimo", "3",
                "Quantidade mínima de folhas por modelo antes de alertar reposição"),

            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110002"), "dtf.custo_folha_padrao", "49.80",
                "Custo padrão de uma folha DTF em reais"),

            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110003"), "pedido.lead_time_padrao_dias", "7",
                "Prazo médio em dias para recebimento de pedidos a fornecedores"),

            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110004"), "estoque.tecido.alerta_minimo_kg", "5",
                "Quantidade mínima em kg de tecido antes de alertar reposição"),

            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110005"), "sistema.timezone", "America/Sao_Paulo",
                "Fuso horário padrão do sistema"),

            Seed(new Guid("aaaaaaaa-0000-0000-0000-000011110006"), "sistema.moeda", "BRL",
                "Moeda padrão do sistema")
        );
    }

    private static object Seed(Guid id, string key, string value, string description) =>
        new
        {
            Id = id,
            Key = key,
            Value = value,
            Description = description,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate,
            IsDeleted = false
        };
}
