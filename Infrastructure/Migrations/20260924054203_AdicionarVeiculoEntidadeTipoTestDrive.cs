using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarVeiculoEntidadeTipoTestDrive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "VeiculoEntidadeTipo",
                table: "TestDrives",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "VeiculoVenda");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VeiculoEntidadeTipo",
                table: "TestDrives");
        }
    }
}
