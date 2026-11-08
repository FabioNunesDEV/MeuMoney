using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Services.Abstractions;

namespace Money.Data.Tests;

public sealed class SqliteProviderTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));

    [Fact]
    public async Task InitializeAsync_CriaArquivoDoBanco()
    {
        var dbPath = Path.Combine(_directory, "teste.db");
        var services = new ServiceCollection().AddMoneySqlite($"Data Source={dbPath}");

        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();

        await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();

        Assert.True(File.Exists(dbPath));
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // Limpeza best-effort de arquivo temporário.
        }
    }
}
