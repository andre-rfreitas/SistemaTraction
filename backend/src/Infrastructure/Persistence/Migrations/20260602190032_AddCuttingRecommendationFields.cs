using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCuttingRecommendationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RecommendationBasedOnOrders",
                table: "CuttingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RecommendationDays",
                table: "CuttingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedPiecesJson",
                table: "CuttingOrders",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RecommendationBasedOnOrders",
                table: "CuttingOrders");

            migrationBuilder.DropColumn(
                name: "RecommendationDays",
                table: "CuttingOrders");

            migrationBuilder.DropColumn(
                name: "RecommendedPiecesJson",
                table: "CuttingOrders");
        }
    }
}
