using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriaEModalidadeAoAnuncioMercadoLivre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriaML",
                table: "AnunciosMercadoLivre",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ListingTypeML",
                table: "AnunciosMercadoLivre",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UltimoErroDetalheTecnico",
                table: "AnunciosMercadoLivre",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaML",
                table: "AnunciosMercadoLivre");

            migrationBuilder.DropColumn(
                name: "ListingTypeML",
                table: "AnunciosMercadoLivre");

            migrationBuilder.DropColumn(
                name: "UltimoErroDetalheTecnico",
                table: "AnunciosMercadoLivre");
        }
    }
}
