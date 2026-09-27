using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModulosAtivos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ModuloConcessionariaAtivo",
                table: "ConfiguracoesSistema",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ModuloOficinaAtivo",
                table: "ConfiguracoesSistema",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModuloConcessionariaAtivo",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "ModuloOficinaAtivo",
                table: "ConfiguracoesSistema");
        }
    }
}
