using Microsoft.EntityFrameworkCore;
using Money.Services.Abstractions;

namespace Money.Data;

public class MoneyDbContext(DbContextOptions<MoneyDbContext> options) : DbContext(options), IUnitOfWork
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeamentos (IEntityTypeConfiguration) ficam nas pastas de cada módulo deste projeto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoneyDbContext).Assembly);
    }
}
