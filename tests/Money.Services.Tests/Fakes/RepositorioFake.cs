// <copyright file="RepositorioFake.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Services.Abstracoes;

namespace Money.Services.Tests.Fakes;

/// <summary>Repositório em memória para testes de serviço.</summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
public sealed class RepositorioFake<T> : IRepositorio<T>
    where T : Entidade
{
    /// <summary>Obtém as entidades guardadas.</summary>
    public List<T> Itens { get; } = [];

    /// <inheritdoc />
    public Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult<IReadOnlyList<T>>(this.Itens.ToList());
    }

    /// <inheritdoc />
    public Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return Task.FromResult(this.Itens.FirstOrDefault(e => e.Id == id));
    }

    /// <inheritdoc />
    public void Adicionar(T entidade)
    {
        this.Itens.Add(entidade);
    }

    /// <inheritdoc />
    public void Remover(T entidade)
    {
        this.Itens.Remove(entidade);
    }
}
