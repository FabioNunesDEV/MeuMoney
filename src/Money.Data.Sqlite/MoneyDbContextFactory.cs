using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Money.Data;

namespace Money.Data.Sqlite;

/// <summary>Usada apenas pelas ferramentas do EF (dotnet ef migrations ...).</summary>
internal sealed class MoneyDbContextFactory : IDesignTimeDbContextFactory<MoneyDbContext>
{
    public MoneyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MoneyDbContext>()
            .UseSqlite(
                "Data Source=meumoney.design.db",
                sqlite => sqlite.MigrationsAssembly(typeof(MoneyDbContextFactory).Assembly.GetName().Name))
            .Options;

        return new MoneyDbContext(options);
    }
}
