using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddShirtStockMovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShirtStockMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StockItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricColorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Delta = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShirtStockMovements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShirtStockMovements_StockItems_StockItemId",
                        column: x => x.StockItemId,
                        principalTable: "StockItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShirtStockMovements_CreatedAt",
                table: "ShirtStockMovements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtStockMovements_FabricColorId",
                table: "ShirtStockMovements",
                column: "FabricColorId");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtStockMovements_StockItemId",
                table: "ShirtStockMovements",
                column: "StockItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShirtStockMovements");
        }
    }
}
