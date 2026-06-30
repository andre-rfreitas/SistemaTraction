using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstampaToSkuCodeAndSeparationItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DtfModelId",
                table: "SkuCodes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estampa",
                table: "SeparationItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_SkuCodes_DtfModelId",
                table: "SkuCodes",
                column: "DtfModelId");

            migrationBuilder.AddForeignKey(
                name: "FK_SkuCodes_DtfModels_DtfModelId",
                table: "SkuCodes",
                column: "DtfModelId",
                principalTable: "DtfModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SkuCodes_DtfModels_DtfModelId",
                table: "SkuCodes");

            migrationBuilder.DropIndex(
                name: "IX_SkuCodes_DtfModelId",
                table: "SkuCodes");

            migrationBuilder.DropColumn(
                name: "DtfModelId",
                table: "SkuCodes");

            migrationBuilder.DropColumn(
                name: "Estampa",
                table: "SeparationItems");
        }
    }
}
