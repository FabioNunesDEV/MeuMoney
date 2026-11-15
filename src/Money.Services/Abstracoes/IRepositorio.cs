// <copyright file="IRepositorio.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Services.Abstracoes;

/// <summary>
/// Acesso às entidades de um tipo. As alterações em entidades obtidas daqui são gravadas por
/// <see cref="IUnidadeDeTrabalho.SalvarAlteracoesAsync"/>.
/// </summary>
/// <typeparam name="T">Tipo da entidade.</typeparam>
public interface IRepositorio<T>
    where T : Entidade
{
    /// <summary>Lista todas as entidades do tipo.</summary>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>As entidades, sem ordem definida.</returns>
    Task<IReadOnlyList<T>> ListarAsync(CancellationToken cancellationToken);

    /// <summary>Busca a entidade pelo Id.</summary>
    /// <param name="id">Id da entidade.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>A entidade, ou nulo se não existir.</returns>
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Marca a entidade para ser incluída na próxima gravação.</summary>
    /// <param name="entidade">Entidade nova.</param>
    void Adicionar(T entidade);

    /// <summary>Marca a entidade para ser excluída na próxima gravação.</summary>
    /// <param name="entidade">Entidade existente.</param>
    void Remover(T entidade);
}
