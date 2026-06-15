using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDtfOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DtfOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DtfOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DtfOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DtfOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DtfModelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SheetQuantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DtfOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DtfOrderItems_DtfModels_DtfModelId",
                        column: x => x.DtfModelId,
                        principalTable: "DtfModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DtfOrderItems_DtfOrders_DtfOrderId",
                        column: x => x.DtfOrderId,
                        principalTable: "DtfOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DtfOrderItems_DtfModelId",
                table: "DtfOrderItems",
                column: "DtfModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DtfOrderItems_DtfOrderId",
                table: "DtfOrderItems",
                column: "DtfOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_DtfOrders_OrderNumber",
                table: "DtfOrders",
                column: "OrderNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DtfOrderItems");

            migrationBuilder.DropTable(
                name: "DtfOrders");
        }
    }
}
