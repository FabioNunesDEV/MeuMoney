using Microsoft.EntityFrameworkCore;
using Money.Services.Abstracoes;

namespace Money.Data;

/// <summary>Contexto do EF Core do aplicativo. Também é a unidade de trabalho usada pelos serviços.</summary>
public class MoneyDbContext(DbContextOptions<MoneyDbContext> options) : DbContext(options), IUnidadeDeTrabalho
{
    /// <inheritdoc />
    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(cancellationToken);

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mapeamentos (IEntityTypeConfiguration) ficam nas pastas de cada módulo deste projeto.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoneyDbContext).Assembly);
    }
}
