using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPartialSewingDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SewingDeliveries_CuttingOrderId",
                table: "SewingDeliveries");

            migrationBuilder.AddColumn<bool>(
                name: "IsPartial",
                table: "SewingDeliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_SewingDeliveries_CuttingOrderId",
                table: "SewingDeliveries",
                column: "CuttingOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SewingDeliveries_CuttingOrderId",
                table: "SewingDeliveries");

            migrationBuilder.DropColumn(
                name: "IsPartial",
                table: "SewingDeliveries");

            migrationBuilder.CreateIndex(
                name: "IX_SewingDeliveries_CuttingOrderId",
                table: "SewingDeliveries",
                column: "CuttingOrderId",
                unique: true);
        }
    }
}
