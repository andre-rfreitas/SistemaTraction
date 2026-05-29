using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuttingDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveredPiecesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CuttingCostTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingDeliveries_CuttingOrders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalTable: "CuttingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuttingDeliveries_CuttingOrderId",
                table: "CuttingDeliveries",
                column: "CuttingOrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CuttingDeliveries");
        }
    }
}
