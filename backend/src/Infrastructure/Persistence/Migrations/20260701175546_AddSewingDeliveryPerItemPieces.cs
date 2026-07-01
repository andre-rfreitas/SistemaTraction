using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSewingDeliveryPerItemPieces : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefectivePiecesByItemJson",
                table: "SewingDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");

            migrationBuilder.AddColumn<string>(
                name: "GoodPiecesByItemJson",
                table: "SewingDeliveries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "{}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefectivePiecesByItemJson",
                table: "SewingDeliveries");

            migrationBuilder.DropColumn(
                name: "GoodPiecesByItemJson",
                table: "SewingDeliveries");
        }
    }
}
