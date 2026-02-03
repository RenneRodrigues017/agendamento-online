using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Barbearia.Migrations
{
    /// <inheritdoc />
    public partial class AtualizandoTributoFoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Funcionarios");

            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Barbeiros");

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Funcionarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Barbeiros",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
