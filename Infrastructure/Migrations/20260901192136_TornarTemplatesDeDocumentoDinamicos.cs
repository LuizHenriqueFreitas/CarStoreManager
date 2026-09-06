using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TornarTemplatesDeDocumentoDinamicos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TemplatesDocumento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataUltimaAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplatesDocumento", x => x.Id);
                });

            // Migra o conteúdo dos 3 templates fixos (se o admin já tinha
            // customizado algum) para registros dinâmicos, ANTES de derrubar as
            // colunas antigas — sem isso, qualquer customização feita nesses
            // campos seria perdida. Só cria o preset se havia texto de verdade
            // (evita presets vazios numa instalação nova que nunca usou os
            // campos fixos). GUIDs fixos gerados uma vez na autoria da migration
            // — mesmo padrão de valor constante usado em outras migrations de
            // dado deste projeto.
            migrationBuilder.Sql(
                "INSERT INTO TemplatesDocumento (Id, Nome, Conteudo, Ativo, DataUltimaAtualizacao, DataCriacao) " +
                "SELECT '2fc88b64-9ea3-442d-a01b-351269b0652b', 'Termo de entrega (padrão)', TemplateTermoEntrega, 1, DataCriacao, DataCriacao " +
                "FROM ConfiguracoesSistema WHERE TRIM(TemplateTermoEntrega) <> '';");

            migrationBuilder.Sql(
                "INSERT INTO TemplatesDocumento (Id, Nome, Conteudo, Ativo, DataUltimaAtualizacao, DataCriacao) " +
                "SELECT '7d388157-48af-48b7-b701-9591c1121da9', 'Contrato de consignação (padrão)', TemplateContratoConsignacao, 1, DataCriacao, DataCriacao " +
                "FROM ConfiguracoesSistema WHERE TRIM(TemplateContratoConsignacao) <> '';");

            migrationBuilder.Sql(
                "INSERT INTO TemplatesDocumento (Id, Nome, Conteudo, Ativo, DataUltimaAtualizacao, DataCriacao) " +
                "SELECT '4cde4b97-7957-4aeb-a544-b4220e0925dc', 'Resposta da financiadora (roteiro)', TemplateRespostaFinanciadora, 1, DataCriacao, DataCriacao " +
                "FROM ConfiguracoesSistema WHERE TRIM(TemplateRespostaFinanciadora) <> '';");

            migrationBuilder.DropColumn(
                name: "TemplateContratoConsignacao",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "TemplateRespostaFinanciadora",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "TemplateTermoEntrega",
                table: "ConfiguracoesSistema");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplatesDocumento");

            migrationBuilder.AddColumn<string>(
                name: "TemplateContratoConsignacao",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateRespostaFinanciadora",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TemplateTermoEntrega",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
