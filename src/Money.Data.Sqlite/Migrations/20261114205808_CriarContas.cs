using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Money.Data.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class CriarContas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GruposConta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GruposConta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InstituicoesFinanceiras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstituicoesFinanceiras", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Contas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CodigoMoeda = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    InstituicaoFinanceiraId = table.Column<Guid>(type: "TEXT", nullable: true),
                    GrupoContaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ContaCaixaId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Favorita = table.Column<bool>(type: "INTEGER", nullable: false),
                    Encerrada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contas_Contas_ContaCaixaId",
                        column: x => x.ContaCaixaId,
                        principalTable: "Contas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contas_GruposConta_GrupoContaId",
                        column: x => x.GrupoContaId,
                        principalTable: "GruposConta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contas_InstituicoesFinanceiras_InstituicaoFinanceiraId",
                        column: x => x.InstituicaoFinanceiraId,
                        principalTable: "InstituicoesFinanceiras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contas_ContaCaixaId",
                table: "Contas",
                column: "ContaCaixaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_GrupoContaId",
                table: "Contas",
                column: "GrupoContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contas_InstituicaoFinanceiraId",
                table: "Contas",
                column: "InstituicaoFinanceiraId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contas");

            migrationBuilder.DropTable(
                name: "GruposConta");

            migrationBuilder.DropTable(
                name: "InstituicoesFinanceiras");
        }
    }
}
