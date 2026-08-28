using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SepararEnderecoClienteEmTabelaPropria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Enderecos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Logradouro = table.Column<string>(type: "TEXT", nullable: false),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Complemento = table.Column<string>(type: "TEXT", nullable: true),
                    Bairro = table.Column<string>(type: "TEXT", nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", nullable: false),
                    Uf = table.Column<string>(type: "TEXT", maxLength: 2, nullable: false),
                    Cep = table.Column<string>(type: "TEXT", maxLength: 8, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enderecos", x => x.Id);
                });

            // Migra os dados existentes: cada cliente vira uma linha em Enderecos,
            // reaproveitando o próprio Id do cliente como Id do endereço — é uma
            // relação 1:1 no momento da migração, então não precisa gerar nem
            // correlacionar novos GUIDs em SQL puro.
            migrationBuilder.Sql(@"
                INSERT INTO Enderecos (Id, Logradouro, Numero, Complemento, Bairro, Cidade, Uf, Cep, DataCriacao)
                SELECT Id, EnderecoLogradouro, EnderecoNumero, EnderecoComplemento, EnderecoBairro, EnderecoCidade, EnderecoUf, EnderecoCep, DataCriacao
                FROM Clientes;
            ");

            migrationBuilder.AddColumn<Guid>(
                name: "EnderecoId",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.Sql("UPDATE Clientes SET EnderecoId = Id;");

            migrationBuilder.DropColumn(name: "EnderecoBairro", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoCep", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoCidade", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoComplemento", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoLogradouro", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoNumero", table: "Clientes");
            migrationBuilder.DropColumn(name: "EnderecoUf", table: "Clientes");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_EnderecoId",
                table: "Clientes",
                column: "EnderecoId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Enderecos_EnderecoId",
                table: "Clientes",
                column: "EnderecoId",
                principalTable: "Enderecos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoBairro",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCep",
                table: "Clientes",
                type: "TEXT",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoCidade",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoComplemento",
                table: "Clientes",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnderecoLogradouro",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoNumero",
                table: "Clientes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EnderecoUf",
                table: "Clientes",
                type: "TEXT",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            // Restaura os dados de volta pras colunas do cliente antes de
            // derrubar a tabela — mesma lógica do Up(), no sentido inverso.
            migrationBuilder.Sql(@"
                UPDATE Clientes
                SET EnderecoLogradouro = (SELECT Logradouro FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoNumero = (SELECT Numero FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoComplemento = (SELECT Complemento FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoBairro = (SELECT Bairro FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoCidade = (SELECT Cidade FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoUf = (SELECT Uf FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId),
                    EnderecoCep = (SELECT Cep FROM Enderecos WHERE Enderecos.Id = Clientes.EnderecoId);
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Enderecos_EnderecoId",
                table: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_EnderecoId",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "EnderecoId",
                table: "Clientes");

            migrationBuilder.DropTable(
                name: "Enderecos");
        }
    }
}
