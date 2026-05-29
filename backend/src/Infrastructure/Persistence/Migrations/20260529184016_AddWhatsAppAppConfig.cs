using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("dddddddd-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Número de WhatsApp do cortador (com DDI, ex: 5511999999999)", false, "wp_cutter_phone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("dddddddd-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nome do cortador exibido na tela de revisão", false, "wp_cutter_name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Cortador" },
                    { new Guid("dddddddd-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Número de WhatsApp do costureiro (com DDI)", false, "wp_sewer_phone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("dddddddd-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nome do costureiro exibido na tela de revisão", false, "wp_sewer_name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Costureiro" },
                    { new Guid("dddddddd-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Número de WhatsApp do fornecedor DTF (com DDI)", false, "wp_dtf_phone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "" },
                    { new Guid("dddddddd-0000-0000-0000-000000000006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nome do fornecedor DTF exibido na tela de revisão", false, "wp_dtf_name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fornecedor DTF" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000001"));
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000002"));
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000003"));
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000004"));
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000005"));
            migrationBuilder.DeleteData(table: "AppConfigs", keyColumn: "Id", keyValue: new Guid("dddddddd-0000-0000-0000-000000000006"));
        }
    }
}
