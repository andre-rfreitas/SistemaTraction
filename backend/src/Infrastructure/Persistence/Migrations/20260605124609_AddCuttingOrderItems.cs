using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CuttingOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricRollId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedPiecesJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingOrderItems_CuttingOrders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalTable: "CuttingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuttingOrderItems_FabricRolls_FabricRollId",
                        column: x => x.FabricRollId,
                        principalTable: "FabricRolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CuttingOrderItems_CuttingOrderId",
                table: "CuttingOrderItems",
                column: "CuttingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingOrderItems_FabricRollId",
                table: "CuttingOrderItems",
                column: "FabricRollId");

            // Migrate existing data: each CuttingOrder becomes one CuttingOrderItem
            migrationBuilder.Sql(@"
                INSERT INTO CuttingOrderItems (Id, CuttingOrderId, FabricRollId, RequestedPiecesJson, CreatedAt, UpdatedAt, IsDeleted)
                SELECT NEWID(), Id, FabricRollId, RequestedPiecesJson, CreatedAt, UpdatedAt, 0
                FROM CuttingOrders
                WHERE IsDeleted = 0
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_CuttingOrders_FabricRolls_FabricRollId",
                table: "CuttingOrders");

            migrationBuilder.DropIndex(
                name: "IX_CuttingOrders_FabricRollId",
                table: "CuttingOrders");

            migrationBuilder.DropColumn(
                name: "FabricRollId",
                table: "CuttingOrders");

            migrationBuilder.DropColumn(
                name: "RequestedPiecesJson",
                table: "CuttingOrders");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FabricRollId",
                table: "CuttingOrders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RequestedPiecesJson",
                table: "CuttingOrders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Restore data from CuttingOrderItems (first item per order)
            migrationBuilder.Sql(@"
                UPDATE o
                SET o.FabricRollId = i.FabricRollId, o.RequestedPiecesJson = i.RequestedPiecesJson
                FROM CuttingOrders o
                INNER JOIN (
                    SELECT CuttingOrderId, FabricRollId, RequestedPiecesJson,
                           ROW_NUMBER() OVER (PARTITION BY CuttingOrderId ORDER BY CreatedAt) AS rn
                    FROM CuttingOrderItems WHERE IsDeleted = 0
                ) i ON o.Id = i.CuttingOrderId AND i.rn = 1
            ");

            migrationBuilder.DropTable(
                name: "CuttingOrderItems");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingOrders_FabricRollId",
                table: "CuttingOrders",
                column: "FabricRollId");

            migrationBuilder.AddForeignKey(
                name: "FK_CuttingOrders_FabricRolls_FabricRollId",
                table: "CuttingOrders",
                column: "FabricRollId",
                principalTable: "FabricRolls",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
