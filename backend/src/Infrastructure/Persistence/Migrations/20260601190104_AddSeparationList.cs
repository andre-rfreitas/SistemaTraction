using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSeparationList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SeparationItems_SeparationLists_SeparationListId1",
                table: "SeparationItems");

            migrationBuilder.DropIndex(
                name: "IX_SeparationItems_SeparationListId1",
                table: "SeparationItems");

            migrationBuilder.DropColumn(
                name: "SeparationListId1",
                table: "SeparationItems");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SeparationListId1",
                table: "SeparationItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SeparationItems_SeparationListId1",
                table: "SeparationItems",
                column: "SeparationListId1");

            migrationBuilder.AddForeignKey(
                name: "FK_SeparationItems_SeparationLists_SeparationListId1",
                table: "SeparationItems",
                column: "SeparationListId1",
                principalTable: "SeparationLists",
                principalColumn: "Id");
        }
    }
}
