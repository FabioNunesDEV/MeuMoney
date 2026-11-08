using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Money.Data;

namespace Money.Data.Sqlite;

/// <summary>Usada apenas pelas ferramentas do EF (dotnet ef migrations ...).</summary>
internal sealed class FabricaMoneyDbContext : IDesignTimeDbContextFactory<MoneyDbContext>
{
    /// <inheritdoc />
    public MoneyDbContext CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<MoneyDbContext>()
            .UseSqlite(
                "Data Source=meumoney.design.db",
                sqlite => sqlite.MigrationsAssembly(typeof(FabricaMoneyDbContext).Assembly.GetName().Name))
            .Options;

        return new MoneyDbContext(opcoes);
    }
}
