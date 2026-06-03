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
            Seed(new Guid("cccccccc-0000-0000-0000-000000000001"),
                "cutting_price_default", "1.00",
                "Preço padrão de corte por peça (R$)"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000002"),
                "sewing_price_default", "5.60",
                "Preço padrão de costura por peça — tamanhos P/M/G/GG (R$)"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000003"),
                "sewing_price_g1", "6.30",
                "Preço de costura por peça — tamanho G1 (R$)"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000004"),
                "dtf_sheet_price_default", "49.80",
                "Preço padrão por folha DTF (R$)"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000005"),
                "stock_alert_threshold", "15",
                "Quantidade mínima em estoque antes de disparar alerta de reposição"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000006"),
                "recommendation_days", "30",
                "Dias de histórico usados para calcular recomendação de corte"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000007"),
                "sizes_available", "P,M,G,G1,GG",
                "Tamanhos disponíveis para produção, separados por vírgula"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000008"),
                "wp_provider", "manual",
                "Provedor de WhatsApp: manual (link wa.me) ou nicochat (API)"),

            Seed(new Guid("cccccccc-0000-0000-0000-000000000009"),
                "wp_nicochat_url", "",
                "URL base da API Nicochat (ex: https://api.nicochat.com)"),

            Seed(new Guid("cccccccc-0000-0000-0000-00000000000a"),
                "wp_nicochat_key", "",
                "API Key da Nicochat"),

            Seed(new Guid("cccccccc-0000-0000-0000-00000000000b"),
                "wp_template_cutter",
                "Pedido #{OrderNumber}\n{Color} {Variation}\n{SizesBlock}\nTotal: {Total} peças",
                "Template da mensagem para o cortador. Variáveis: {OrderNumber}, {Color}, {Variation}, {SizesBlock}, {Total}"),

            Seed(new Guid("cccccccc-0000-0000-0000-00000000000c"),
                "wp_template_sewer",
                "Pedido {OrderNumber}\n{Color} {Variation} - {Total}\n{SizesBlock}\nTotal {Total} camisetas R${Cost}",
                "Template da mensagem para o costureiro. Variáveis: {OrderNumber}, {Color}, {Variation}, {Total}, {SizesBlock}, {Cost}"),

            Seed(new Guid("cccccccc-0000-0000-0000-00000000000d"),
                "wp_template_dtf",
                "Pedido DTF - {Date}\n{SheetsBlock}\nTotal: {TotalSheets} folha(s) — R${TotalCost}",
                "Template da mensagem para o fornecedor DTF. Variáveis: {Date}, {SheetsBlock}, {TotalSheets}, {TotalCost}"),

            Seed(new Guid("cccccccc-0000-0000-0000-00000000000e"),
                "dtf_stock_alert_threshold", "100",
                "Quantidade mínima de estampas DTF antes de disparar alerta de reposição")
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
