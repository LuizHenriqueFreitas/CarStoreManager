using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SistemaDespesasMensais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Despesas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiaFechamentoDespesas",
                table: "ConfiguracoesSistema",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.CreateTable(
                name: "BalancosMensaisDespesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Competencia = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Fechado = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BalancosMensaisDespesa", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemBalancoDespesa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BalancoId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Setor = table.Column<string>(type: "TEXT", nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: true),
                    Valor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    DoModelo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemBalancoDespesa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ItemBalancoDespesa_BalancosMensaisDespesa_BalancoId",
                        column: x => x.BalancoId,
                        principalTable: "BalancosMensaisDespesa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BalancosMensaisDespesa_Competencia",
                table: "BalancosMensaisDespesa",
                column: "Competencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemBalancoDespesa_BalancoId",
                table: "ItemBalancoDespesa",
                column: "BalancoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ItemBalancoDespesa");

            migrationBuilder.DropTable(
                name: "BalancosMensaisDespesa");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Despesas");

            migrationBuilder.DropColumn(
                name: "DiaFechamentoDespesas",
                table: "ConfiguracoesSistema");
        }
    }
}
