using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoTabelaFuncionarioEBarbeiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Barbeiros");

            migrationBuilder.AddColumn<string>(
                name: "ImagemPath",
                table: "Funcionarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagemPath",
                table: "Barbeiros",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemPath",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "ImagemPath",
                table: "Barbeiros");

            migrationBuilder.AddColumn<byte[]>(
                name: "Foto",
                table: "Funcionarios",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Foto",
                table: "Barbeiros",
                type: "varbinary(max)",
                nullable: true);
        }
    }
}
