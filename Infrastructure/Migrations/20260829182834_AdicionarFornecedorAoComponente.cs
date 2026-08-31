using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarFornecedorAoComponente : Migration
    {
        // CNPJ válido (dígito verificador correto) reservado só pra este
        // fornecedor-placeholder — não é um CNPJ real de mercado.
        private const string PlaceholderId = "00000000-0000-0000-0000-000000000001";
        private const string PlaceholderCnpj = "11122233000183";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FornecedorId",
                table: "Componentes",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Componentes cadastrados antes de o fornecedor virar obrigatório
            // precisam de um valor real pra FK não quebrar — cria um
            // fornecedor-placeholder e aponta todo componente existente pra
            // ele. Só insere se já existir ao menos 1 componente (banco novo
            // não ganha esse registro à toa).
            migrationBuilder.Sql($@"
                INSERT INTO Fornecedores (Id, Nome, CNPJ, EnderecoId, Email, Telefone, Ativo, DataCriacao)
                SELECT '{PlaceholderId}', 'Fornecedor não informado (migração)', '{PlaceholderCnpj}', NULL, NULL, NULL, 1, CURRENT_TIMESTAMP
                WHERE EXISTS (SELECT 1 FROM Componentes)
                  AND NOT EXISTS (SELECT 1 FROM Fornecedores WHERE Id = '{PlaceholderId}');
            ");

            migrationBuilder.Sql($@"
                UPDATE Componentes
                SET FornecedorId = '{PlaceholderId}'
                WHERE FornecedorId = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Componentes_FornecedorId",
                table: "Componentes",
                column: "FornecedorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Componentes_Fornecedores_FornecedorId",
                table: "Componentes",
                column: "FornecedorId",
                principalTable: "Fornecedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Componentes_Fornecedores_FornecedorId",
                table: "Componentes");

            migrationBuilder.DropIndex(
                name: "IX_Componentes_FornecedorId",
                table: "Componentes");

            migrationBuilder.DropColumn(
                name: "FornecedorId",
                table: "Componentes");

            migrationBuilder.Sql($"DELETE FROM Fornecedores WHERE Id = '{PlaceholderId}';");
        }
    }
}
