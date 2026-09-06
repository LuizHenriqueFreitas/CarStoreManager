using CarStoreManager.Domain.Entities.Sistema;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoverSimulacaoFinanciamentoDaProposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParcelasFinanciamento",
                table: "PropostasVenda");

            migrationBuilder.DropColumn(
                name: "TaxaJurosMensal",
                table: "PropostasVenda");

            migrationBuilder.DropColumn(
                name: "ValorParcela",
                table: "PropostasVenda");

            migrationBuilder.RenameColumn(
                name: "ObservacoesFinanciamento",
                table: "PropostasVenda",
                newName: "PropostaFinanciadoraTexto");

            // Adiciona vazia (SQLite recusa ALTER TABLE ADD COLUMN com DEFAULT
            // não-constante — um texto com quebras de linha vira concatenação
            // de CHAR(10) na geração do EF, que o SQLite rejeita como
            // "non-constant default") e faz o backfill do texto padrão real
            // via UPDATE logo em seguida — mesmo motivo da migration
            // AddTemplatesDocumentos (quem já tinha o registro singleton de
            // ConfiguracaoSistema não pode ficar com o template vazio).
            migrationBuilder.AddColumn<string>(
                name: "TemplateRespostaFinanciadora",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                "UPDATE ConfiguracoesSistema SET TemplateRespostaFinanciadora = " +
                "'" + TemplatesDocumentosPadrao.RespostaFinanciadora.Replace("'", "''") + "' " +
                "WHERE TemplateRespostaFinanciadora = '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateRespostaFinanciadora",
                table: "ConfiguracoesSistema");

            migrationBuilder.RenameColumn(
                name: "PropostaFinanciadoraTexto",
                table: "PropostasVenda",
                newName: "ObservacoesFinanciamento");

            migrationBuilder.AddColumn<int>(
                name: "ParcelasFinanciamento",
                table: "PropostasVenda",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TaxaJurosMensal",
                table: "PropostasVenda",
                type: "TEXT",
                precision: 7,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValorParcela",
                table: "PropostasVenda",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);
        }
    }
}
