using Microsoft.EntityFrameworkCore;
using Money.Services.Abstractions;

namespace Money.Data;

internal sealed class MoneyDatabaseInitializer(MoneyDbContext dbContext) : IDatabaseInitializer
{
    public Task InitializeAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
