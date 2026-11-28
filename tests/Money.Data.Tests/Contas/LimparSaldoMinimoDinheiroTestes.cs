// <copyright file="LimparSaldoMinimoDinheiroTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Domain.Contas;
using Money.Services.Abstracoes;

namespace Money.Data.Tests.Contas;

/// <summary>Testes da migration que apaga o saldo mínimo das contas Dinheiro.</summary>
public sealed class LimparSaldoMinimoDinheiroTestes : IDisposable
{
    private const string MigrationAnterior = "20261122160602_AdicionarDetalhesConta";

    private readonly string pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));

    /// <summary>A migration apaga o saldo mínimo da Dinheiro e mantém o da Corrente.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Migrar_DinheiroComSaldoMinimo_ApagaSoDaDinheiro()
    {
        await using var provider = new ServiceCollection()
            .AddMoneySqlite($"Data Source={Path.Combine(this.pasta, "teste.db")}")
            .BuildServiceProvider();
        var dinheiroId = Guid.CreateVersion7();
        var correnteId = Guid.CreateVersion7();

        // Banco como estava antes da migration, com saldo mínimo gravado nos dois tipos.
        await using (var escopo = provider.CreateAsyncScope())
        {
            var banco = escopo.ServiceProvider.GetRequiredService<MoneyDbContext>().Database;
            await banco.GetService<IMigrator>().MigrateAsync(MigrationAnterior);
            await InserirAsync(banco, dinheiroId, "Carteira", "Dinheiro", "50.00");
            await InserirAsync(banco, correnteId, "Banco", "Corrente", "-500.00");
        }

        await using (var escopo = provider.CreateAsyncScope())
        {
            await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();
        }

        await using (var escopo = provider.CreateAsyncScope())
        {
            var contas = escopo.ServiceProvider.GetRequiredService<IRepositorio<Conta>>();
            Assert.Null((await contas.ObterPorIdAsync(dinheiroId, CancellationToken.None))?.SaldoMinimo);
            Assert.Equal(-500m, (await contas.ObterPorIdAsync(correnteId, CancellationToken.None))?.SaldoMinimo);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(this.pasta, recursive: true);
        }
        catch (IOException)
        {
            // Limpeza best-effort de arquivo temporário.
        }
    }

    private static async Task InserirAsync(DatabaseFacade banco, Guid id, string nome, string tipo, string saldoMinimo)
    {
        // Guid em maiúsculas, como o provedor do SQLite grava.
        var textoId = id.ToString().ToUpperInvariant();
        await banco.ExecuteSqlAsync(
            $"INSERT INTO Contas (Id, Nome, Tipo, CodigoMoeda, SaldoInicial, Favorita, Encerrada, SaldoMinimo) VALUES ({textoId}, {nome}, {tipo}, 'BRL', '0.0', 0, 0, {saldoMinimo})");
    }
}
