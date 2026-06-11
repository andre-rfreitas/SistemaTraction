using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplyMovementDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PricePerUnit",
                table: "SupplyTypes",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OccurredAt",
                table: "SupplyStockMovements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "SupplierName",
                table: "SupplyStockMovements",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupplierPhone",
                table: "SupplyStockMovements",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCost",
                table: "SupplyStockMovements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "SupplyStockMovements",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PricePerUnit",
                table: "SupplyTypes");

            migrationBuilder.DropColumn(
                name: "OccurredAt",
                table: "SupplyStockMovements");

            migrationBuilder.DropColumn(
                name: "SupplierName",
                table: "SupplyStockMovements");

            migrationBuilder.DropColumn(
                name: "SupplierPhone",
                table: "SupplyStockMovements");

            migrationBuilder.DropColumn(
                name: "TotalCost",
                table: "SupplyStockMovements");

            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "SupplyStockMovements");
        }
    }
}
