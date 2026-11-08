using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Services.Abstracoes;

namespace Money.Data.Tests;

public sealed class ProvedorSqliteTestes : IDisposable
{
    private readonly string _pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task InicializarAsync_CriaArquivoDoBanco()
    {
        var caminhoBanco = Path.Combine(_pasta, "teste.db");
        var servicos = new ServiceCollection().AddMoneySqlite($"Data Source={caminhoBanco}");

        await using var provider = servicos.BuildServiceProvider();
        await using var escopo = provider.CreateAsyncScope();

        await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();

        Assert.True(File.Exists(caminhoBanco));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_pasta, recursive: true);
        }
        catch (IOException)
        {
            // Limpeza best-effort de arquivo temporário.
        }
    }
}
