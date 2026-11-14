// <copyright file="InicializadorBanco.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Money.Services.Abstracoes;

namespace Money.Data;

/// <summary>Aplica as migrations do EF Core na inicialização do aplicativo.</summary>
/// <param name="dbContext">Contexto cujo banco será migrado.</param>
internal sealed class InicializadorBanco(MoneyDbContext dbContext) : IInicializadorBanco
{
    /// <inheritdoc />
    public Task InicializarAsync(CancellationToken cancellationToken = default)
        => dbContext.Database.MigrateAsync(cancellationToken);
}
