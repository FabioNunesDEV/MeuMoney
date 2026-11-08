using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Data;

namespace Money.Data.Sqlite;

public static class SqliteDependencyInjection
{
    /// <summary>Usa SQLite como banco. Expande variáveis de ambiente e cria a pasta do arquivo, se necessário.</summary>
    public static IServiceCollection AddMoneySqlite(this IServiceCollection services, string connectionString)
    {
        var builder = new SqliteConnectionStringBuilder(Environment.ExpandEnvironmentVariables(connectionString));
        EnsureDirectory(builder);

        return services.AddMoneyData(options => options.UseSqlite(
            builder.ConnectionString,
            sqlite => sqlite.MigrationsAssembly(typeof(SqliteDependencyInjection).Assembly.GetName().Name)));
    }

    private static void EnsureDirectory(SqliteConnectionStringBuilder builder)
    {
        if (builder.Mode == SqliteOpenMode.Memory || builder.DataSource == ":memory:" || string.IsNullOrWhiteSpace(builder.DataSource))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(builder.DataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
