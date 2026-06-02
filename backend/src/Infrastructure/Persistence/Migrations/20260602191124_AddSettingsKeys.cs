using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("cccccccc-0000-0000-0000-000000000008"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Provedor de WhatsApp: manual (link wa.me) ou nicochat (API)", false, "wp_provider", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "manual" },
                    { new Guid("cccccccc-0000-0000-0000-000000000009"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "URL base da API Nicochat (ex: https://api.nicochat.com)", false, "wp_nicochat_url", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("cccccccc-0000-0000-0000-00000000000a"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "API Key da Nicochat", false, "wp_nicochat_key", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("cccccccc-0000-0000-0000-00000000000b"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Template da mensagem para o cortador. Variáveis: {OrderNumber}, {Color}, {Variation}, {SizesBlock}, {Total}", false, "wp_template_cutter", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pedido #{OrderNumber}\n{Color} {Variation}\n{SizesBlock}\nTotal: {Total} peças" },
                    { new Guid("cccccccc-0000-0000-0000-00000000000c"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Template da mensagem para o costureiro. Variáveis: {OrderNumber}, {Color}, {Variation}, {Total}, {SizesBlock}, {Cost}", false, "wp_template_sewer", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pedido {OrderNumber}\n{Color} {Variation} - {Total}\n{SizesBlock}\nTotal {Total} camisetas R${Cost}" },
                    { new Guid("cccccccc-0000-0000-0000-00000000000d"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Template da mensagem para o fornecedor DTF. Variáveis: {Date}, {SheetsBlock}, {TotalSheets}, {TotalCost}", false, "wp_template_dtf", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pedido DTF - {Date}\n{SheetsBlock}\nTotal: {TotalSheets} folha(s) — R${TotalCost}" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000a"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000b"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000c"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000d"));
        }
    }
}
