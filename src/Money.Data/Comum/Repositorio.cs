// <copyright file="Repositorio.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Money.Domain.Comum;
using Money.Services.Abstracoes;

namespace Money.Data.Comum;

/// <summary>Repositório de um tipo de entidade sobre o <see cref="MoneyDbContext"/>.</summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
/// <param name="dbContext">Contexto do escopo atual; grava as alterações rastreadas.</param>
internal class Repositorio<T>(MoneyDbContext dbContext) : IRepositorio<T>
    where T : Entidade
{
    /// <summary>Obtém o contexto do escopo atual, para as consultas próprias dos repositórios derivados.</summary>
    protected MoneyDbContext DbContext => dbContext;

    /// <inheritdoc />
    public async Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Set<T>().ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Set<T>().FindAsync([id], cancellationToken);
    }

    /// <inheritdoc />
    public void Adicionar(T entidade)
    {
        dbContext.Set<T>().Add(entidade);
    }

    /// <inheritdoc />
    public void Remover(T entidade)
    {
        dbContext.Set<T>().Remove(entidade);
    }
}
