using CarStoreManager.Domain.Entities.Sistema;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTemplatesDocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // defaultValue usa o mesmo texto padrão de TemplatesDocumentosPadrao (não
            // uma string vazia) — sem isso, quem já tinha o registro singleton de
            // ConfiguracaoSistema criado antes desta migration ficaria com os
            // templates vazios, já que o inicializador de propriedade do C# só roda
            // ao construir um objeto novo, não ao aplicar uma coluna em uma linha
            // já existente no banco.
            migrationBuilder.AddColumn<string>(
                name: "TemplateContratoConsignacao",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: TemplatesDocumentosPadrao.ContratoConsignacao);

            migrationBuilder.AddColumn<string>(
                name: "TemplateTermoEntrega",
                table: "ConfiguracoesSistema",
                type: "TEXT",
                nullable: false,
                defaultValue: TemplatesDocumentosPadrao.TermoEntrega);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemplateContratoConsignacao",
                table: "ConfiguracoesSistema");

            migrationBuilder.DropColumn(
                name: "TemplateTermoEntrega",
                table: "ConfiguracoesSistema");
        }
    }
}
