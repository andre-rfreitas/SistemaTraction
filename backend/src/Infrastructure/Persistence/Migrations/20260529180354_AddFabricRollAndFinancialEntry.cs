using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFabricRollAndFinancialEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FabricRolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FabricColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WeightKg = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    PriceTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PricePerKgActual = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FabricRolls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FabricRolls_FabricColors_FabricColorId",
                        column: x => x.FabricColorId,
                        principalTable: "FabricColors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FabricRolls_FabricTypes_FabricTypeId",
                        column: x => x.FabricTypeId,
                        principalTable: "FabricTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReferenceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FabricRolls_FabricColorId",
                table: "FabricRolls",
                column: "FabricColorId");

            migrationBuilder.CreateIndex(
                name: "IX_FabricRolls_FabricTypeId",
                table: "FabricRolls",
                column: "FabricTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEntries_Category",
                table: "FinancialEntries",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialEntries_EntryDate",
                table: "FinancialEntries",
                column: "EntryDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FabricRolls");

            migrationBuilder.DropTable(
                name: "FinancialEntries");
        }
    }
}
