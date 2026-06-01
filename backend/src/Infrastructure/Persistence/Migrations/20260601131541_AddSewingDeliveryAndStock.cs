using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingDeliveryAndStock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SewingDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GoodPiecesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefectivePiecesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SewingCostTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    DefectCostTotal = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SewingDeliveries_CuttingOrders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalTable: "CuttingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricColorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FabricTypeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FabricTypeVariation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Size = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SewingDeliveries_CuttingOrderId",
                table: "SewingDeliveries",
                column: "CuttingOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockItems_FabricColorId_Size",
                table: "StockItems",
                columns: new[] { "FabricColorId", "Size" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SewingDeliveries");

            migrationBuilder.DropTable(
                name: "StockItems");
        }
    }
}
