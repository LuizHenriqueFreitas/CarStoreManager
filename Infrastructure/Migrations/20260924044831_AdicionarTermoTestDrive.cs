using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarTermoTestDrive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TermosTestDrive",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TestDriveId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TextoTermo = table.Column<string>(type: "TEXT", nullable: false),
                    VendedorRedatorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DataRedacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataUltimaEdicao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    TokenAssinatura = table.Column<string>(type: "TEXT", nullable: true),
                    DataAssinatura = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AssinaturaNomeCliente = table.Column<string>(type: "TEXT", nullable: true),
                    AssinaturaCpfCliente = table.Column<string>(type: "TEXT", nullable: true),
                    AssinaturaIp = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TermosTestDrive", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TermosTestDrive_TestDriveId",
                table: "TermosTestDrive",
                column: "TestDriveId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TermosTestDrive_TokenAssinatura",
                table: "TermosTestDrive",
                column: "TokenAssinatura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TermosTestDrive");
        }
    }
}
