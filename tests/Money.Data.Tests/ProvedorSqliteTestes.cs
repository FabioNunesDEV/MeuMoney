// <copyright file="ProvedorSqliteTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Services.Abstracoes;

namespace Money.Data.Tests;

/// <summary>Testes do registro do SQLite como provedor de banco.</summary>
public sealed class ProvedorSqliteTestes : IDisposable
{
    private readonly string pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));

    /// <summary>Inicializar o banco cria o arquivo no caminho da string de conexão.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task InicializarAsync_CriaArquivoDoBanco()
    {
        var caminhoBanco = Path.Combine(this.pasta, "teste.db");
        var servicos = new ServiceCollection().AddMoneySqlite($"Data Source={caminhoBanco}");

        await using var provider = servicos.BuildServiceProvider();
        await using var escopo = provider.CreateAsyncScope();

        await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();

        Assert.True(File.Exists(caminhoBanco));
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
}
