using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVeiculoEntidadeTipoProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue "VeiculoVenda" (não string vazia) — toda proposta já
            // existente antes desta coluna existir é, por definição, sobre um
            // veículo de estoque próprio (o suporte a consignado é novo).
            migrationBuilder.AddColumn<string>(
                name: "VeiculoEntidadeTipo",
                table: "PropostasVenda",
                type: "TEXT",
                nullable: false,
                defaultValue: "VeiculoVenda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VeiculoEntidadeTipo",
                table: "PropostasVenda");
        }
    }
}
