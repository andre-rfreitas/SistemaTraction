using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShopifyConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("cccccccc-0000-0000-0000-00000000000f"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "URL da loja Shopify (ex: minha-loja.myshopify.com)", false, "shopify_store_url", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("cccccccc-0000-0000-0000-000000000010"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Token de acesso da API privada Shopify", false, "shopify_access_token", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("cccccccc-0000-0000-0000-000000000011"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Versão da Shopify Admin API", false, "shopify_api_version", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2026-01" },
                    { new Guid("cccccccc-0000-0000-0000-000000000012"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Data/hora do último sync de pedidos Shopify", false, "shopify_last_sync", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("cccccccc-0000-0000-0000-000000000013"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade de pedidos importados no último sync", false, "shopify_last_sync_imported", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0" },
                    { new Guid("cccccccc-0000-0000-0000-000000000014"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Valor total importado no último sync (R$)", false, "shopify_last_sync_amount", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "0" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000f"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-000000000014"));
        }
    }
}
