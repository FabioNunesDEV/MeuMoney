using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Sqlite.Migrations
{
    /// <summary>Apaga o saldo mínimo das contas Dinheiro, tipo que deixou de ter saldo mínimo.</summary>
    public partial class LimparSaldoMinimoDinheiro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE Contas SET SaldoMinimo = NULL WHERE Tipo = 'Dinheiro'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nada a desfazer: o saldo mínimo apagado não é guardado em outro lugar e não volta.
        }
    }
}
