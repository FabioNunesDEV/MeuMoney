using Microsoft.EntityFrameworkCore;
using Money.Services.Abstracoes;

namespace Money.Data;

/// <summary>Aplica as migrations do EF Core na inicialização do aplicativo.</summary>
internal sealed class InicializadorBanco(MoneyDbContext dbContext) : IInicializadorBanco
{
    /// <inheritdoc />
    public Task InicializarAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
