using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarValorAquisicaoEIpvaObrigatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ADD COLUMN simples (não exige rebuild de tabela no SQLite) — nasce
            // com default 0m só para satisfazer o NOT NULL até o backfill abaixo.
            migrationBuilder.AddColumn<decimal>(
                name: "ValorAquisicao",
                table: "VeiculosVenda",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);

            // Backfill: sem histórico real de compra pros veículos já cadastrados,
            // estima o valor de aquisição em 75% do valor de venda (Valor já é
            // armazenado como texto decimal nesta tabela — mesmo formato aqui).
            migrationBuilder.Sql(@"
                UPDATE VeiculosVenda
                SET ValorAquisicao = CAST(ROUND(CAST(Valor AS REAL) * 0.75, 2) AS TEXT);
            ");

            // Backfill: assume que o IPVA do ano de fabricação/cadastro já foi
            // pago — estimativa razoável na ausência de um valor real informado.
            migrationBuilder.Sql(@"
                UPDATE VeiculosVenda
                SET AnoUltimoIpvaPago = Ano
                WHERE AnoUltimoIpvaPago IS NULL;
            ");

            // Só agora, com todas as linhas já preenchidas, vira NOT NULL
            // (dispara o rebuild de tabela do provider SQLite).
            migrationBuilder.AlterColumn<int>(
                name: "AnoUltimoIpvaPago",
                table: "VeiculosVenda",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorAquisicao",
                table: "VeiculosVenda");

            migrationBuilder.AlterColumn<int>(
                name: "AnoUltimoIpvaPago",
                table: "VeiculosVenda",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
