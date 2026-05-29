using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SistemaTraction.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAppConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppConfigs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AppConfigs",
                columns: new[] { "Id", "CreatedAt", "Description", "IsDeleted", "Key", "UpdatedAt", "Value" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima de folhas por modelo antes de alertar reposição", false, "dtf.alerta_estoque_minimo", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Custo padrão de uma folha DTF em reais", false, "dtf.custo_folha_padrao", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "49.80" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Prazo médio em dias para recebimento de pedidos a fornecedores", false, "pedido.lead_time_padrao_dias", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "7" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Quantidade mínima em kg de tecido antes de alertar reposição", false, "estoque.tecido.alerta_minimo_kg", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Fuso horário padrão do sistema", false, "sistema.timezone", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "America/Sao_Paulo" },
                    { new Guid("aaaaaaaa-0000-0000-0000-000011110006"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Moeda padrão do sistema", false, "sistema.moeda", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "BRL" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppConfigs_Key",
                table: "AppConfigs",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppConfigs");
        }
    }
}
