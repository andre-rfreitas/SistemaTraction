using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDtfSheetCountAndDtfThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SheetCount",
                table: "DtfStockMovements",
                type: "int",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[] { new Guid("cccccccc-0000-0000-0000-00000000000e"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima de estampas DTF antes de disparar alerta de reposição", false, "dtf_stock_alert_threshold", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "100" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppConfigs",
                keyColumn: "Id",
                keyValue: new Guid("cccccccc-0000-0000-0000-00000000000e"));

            migrationBuilder.DropColumn(
                name: "SheetCount",
                table: "DtfStockMovements");
        }
    }
}
