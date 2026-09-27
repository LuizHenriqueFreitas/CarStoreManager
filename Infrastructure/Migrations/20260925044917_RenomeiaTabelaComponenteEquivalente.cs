using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarStoreManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenomeiaTabelaComponenteEquivalente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponenteEquivalente_Componentes_ComponenteEquivalenteId",
                table: "ComponenteEquivalente");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponenteEquivalente_Componentes_ComponenteOriginalId",
                table: "ComponenteEquivalente");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComponenteEquivalente",
                table: "ComponenteEquivalente");

            migrationBuilder.RenameTable(
                name: "ComponenteEquivalente",
                newName: "ComponentesEquivalentes");

            migrationBuilder.RenameIndex(
                name: "IX_ComponenteEquivalente_ComponenteOriginalId",
                table: "ComponentesEquivalentes",
                newName: "IX_ComponentesEquivalentes_ComponenteOriginalId");

            migrationBuilder.RenameIndex(
                name: "IX_ComponenteEquivalente_ComponenteEquivalenteId",
                table: "ComponentesEquivalentes",
                newName: "IX_ComponentesEquivalentes_ComponenteEquivalenteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComponentesEquivalentes",
                table: "ComponentesEquivalentes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentesEquivalentes_Componentes_ComponenteEquivalenteId",
                table: "ComponentesEquivalentes",
                column: "ComponenteEquivalenteId",
                principalTable: "Componentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponentesEquivalentes_Componentes_ComponenteOriginalId",
                table: "ComponentesEquivalentes",
                column: "ComponenteOriginalId",
                principalTable: "Componentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComponentesEquivalentes_Componentes_ComponenteEquivalenteId",
                table: "ComponentesEquivalentes");

            migrationBuilder.DropForeignKey(
                name: "FK_ComponentesEquivalentes_Componentes_ComponenteOriginalId",
                table: "ComponentesEquivalentes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ComponentesEquivalentes",
                table: "ComponentesEquivalentes");

            migrationBuilder.RenameTable(
                name: "ComponentesEquivalentes",
                newName: "ComponenteEquivalente");

            migrationBuilder.RenameIndex(
                name: "IX_ComponentesEquivalentes_ComponenteOriginalId",
                table: "ComponenteEquivalente",
                newName: "IX_ComponenteEquivalente_ComponenteOriginalId");

            migrationBuilder.RenameIndex(
                name: "IX_ComponentesEquivalentes_ComponenteEquivalenteId",
                table: "ComponenteEquivalente",
                newName: "IX_ComponenteEquivalente_ComponenteEquivalenteId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ComponenteEquivalente",
                table: "ComponenteEquivalente",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ComponenteEquivalente_Componentes_ComponenteEquivalenteId",
                table: "ComponenteEquivalente",
                column: "ComponenteEquivalenteId",
                principalTable: "Componentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ComponenteEquivalente_Componentes_ComponenteOriginalId",
                table: "ComponenteEquivalente",
                column: "ComponenteOriginalId",
                principalTable: "Componentes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
