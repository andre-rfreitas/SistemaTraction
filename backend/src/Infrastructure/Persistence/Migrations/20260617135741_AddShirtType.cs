using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShirtType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_FabricColorId_Size",
                table: "StockItems");

            migrationBuilder.AddColumn<string>(
                name: "ShirtType",
                table: "StockItems",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Regular");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockItems_FabricColorId_Size_ShirtType",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ShirtType",
                table: "StockItems");

            migrationBuilder.DropColumn(
                name: "ShirtType",
                table: "ShirtStockMovements");

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_FabricColorId_Size",
                table: "StockItems",
                columns: new[] { "FabricColorId", "Size" },
                unique: true);
        }
    }
}
