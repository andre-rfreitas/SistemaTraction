using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceShirtTypeWithModelCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_FabricColorId_Size_ShirtType",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ShirtType",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "DtfModelId",
                table: "SkuCodes");

            migrationBuilder.DropColumn(
                name: "ShirtType",
                table: "ShirtStockMovements");

            migrationBuilder.AddColumn<string>(
                name: "ModelCode",
                table: "StockItems",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "REG");

            migrationBuilder.AddColumn<string>(
                name: "ModelCode",
                table: "ShirtStockMovements",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "REG");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_FabricColorId_Size_ModelCode",
                table: "StockItems",
                columns: new[] { "FabricColorId", "Size", "ModelCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_FabricColorId_Size_ModelCode",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ModelCode",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ModelCode",
                table: "ShirtStockMovements");

            migrationBuilder.AddColumn<string>(
                name: "ShirtType",
                table: "StockItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.AddColumn<Guid>(
                name: "DtfModelId",
                table: "SkuCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtType",
                table: "ShirtStockMovements",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_FabricColorId_Size_ShirtType",
                table: "StockItems",
                columns: new[] { "FabricColorId", "Size", "ShirtType" },
                unique: true);
        }
    }
}
