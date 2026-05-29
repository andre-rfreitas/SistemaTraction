using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceAppConfigSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110001"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110002"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110003"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110004"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110005"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("aaaaaaaa-0000-0000-0000-000011110006"));

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("cccccccc-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Preço padrão de corte por peça (R$)", false, "cutting_price_default", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.00" },
                    { new Guid("cccccccc-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Preço padrão de costura por peça — tamanhos P/M/G/GG (R$)", false, "sewing_price_default", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5.60" },
                    { new Guid("cccccccc-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Preço de costura por peça — tamanho G1 (R$)", false, "sewing_price_g1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "6.30" },
                    { new Guid("cccccccc-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Preço padrão por folha DTF (R$)", false, "dtf_sheet_price_default", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "49.80" },
                    { new Guid("cccccccc-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima em estoque antes de disparar alerta de reposição", false, "stock_alert_threshold", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "15" },
                    { new Guid("cccccccc-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Dias de histórico usados para calcular recomendação de corte", false, "recommendation_days", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30" },
                    { new Guid("cccccccc-0000-0000-0000-000000000007"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Tamanhos disponíveis para produção, separados por vírgula", false, "sizes_available", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "P,M,G,G1,GG" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000007"));

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima de folhas por modelo antes de alertar reposição", false, "dtf.alerta_estoque_minimo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Custo padrão de uma folha DTF em reais", false, "dtf.custo_folha_padrao", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "49.80" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Prazo médio em dias para recebimento de pedidos a fornecedores", false, "pedido.lead_time_padrao_dias", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "7" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima em kg de tecido antes de alertar reposição", false, "estoque.tecido.alerta_minimo_kg", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fuso horário padrão do sistema", false, "sistema.timezone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "America/Sao_Paulo" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Moeda padrão do sistema", false, "sistema.moeda", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BRL" }
                });
        }
    }
}
