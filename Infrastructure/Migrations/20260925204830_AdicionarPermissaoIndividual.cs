using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarPermissaoIndividual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PermissoesIndividuais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecursoChave = table.Column<string>(type: "TEXT", nullable: false),
                    Permitido = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermissoesIndividuais", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PermissoesIndividuais_UsuarioId_RecursoChave",
                table: "PermissoesIndividuais",
                columns: new[] { "UsuarioId", "RecursoChave" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermissoesIndividuais");
        }
    }
}
