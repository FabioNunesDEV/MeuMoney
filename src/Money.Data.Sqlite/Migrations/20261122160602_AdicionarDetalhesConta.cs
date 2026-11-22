using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDetalhesConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Abreviacao",
                table: "Contas",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comentario",
                table: "Contas",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroConta",
                table: "Contas",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SaldoMinimo",
                table: "Contas",
                type: "TEXT",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Abreviacao",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "Comentario",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "NumeroConta",
                table: "Contas");

            migrationBuilder.DropColumn(
                name: "SaldoMinimo",
                table: "Contas");
        }
    }
}
