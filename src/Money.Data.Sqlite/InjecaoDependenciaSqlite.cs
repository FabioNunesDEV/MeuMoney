using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Data;

namespace Money.Data.Sqlite;

/// <summary>Registro do SQLite como provedor de banco.</summary>
public static class InjecaoDependenciaSqlite
{
    /// <summary>Usa SQLite como banco. Expande variáveis de ambiente e cria a pasta do arquivo, se necessário.</summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <param name="stringConexao">String de conexão do SQLite; aceita variáveis de ambiente, como %LOCALAPPDATA%.</param>
    /// <returns>A mesma coleção, para encadear registros.</returns>
    public static IServiceCollection AddMoneySqlite(this IServiceCollection servicos, string stringConexao)
    {
        var conexao = new SqliteConnectionStringBuilder(Environment.ExpandEnvironmentVariables(stringConexao));
        GarantirPasta(conexao);

        return servicos.AddMoneyData(opcoes => opcoes.UseSqlite(
            conexao.ConnectionString,
            sqlite => sqlite.MigrationsAssembly(typeof(InjecaoDependenciaSqlite).Assembly.GetName().Name)));
    }

    /// <summary>Cria a pasta do arquivo de banco. Não faz nada para banco em memória.</summary>
    /// <param name="conexao">String de conexão já expandida.</param>
    private static void GarantirPasta(SqliteConnectionStringBuilder conexao)
    {
        if (conexao.Mode == SqliteOpenMode.Memory || conexao.DataSource == ":memory:" || string.IsNullOrWhiteSpace(conexao.DataSource))
        {
            return;
        }

        var pasta = Path.GetDirectoryName(Path.GetFullPath(conexao.DataSource));
        if (!string.IsNullOrEmpty(pasta))
        {
            Directory.CreateDirectory(pasta);
        }
    }
}
