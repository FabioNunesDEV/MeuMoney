// <copyright file="Nomes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Services.Abstracoes;

namespace Money.Services.Comum;

/// <summary>Comparação, unicidade e criação pelo nome dos cadastros.</summary>
internal static class Nomes
{
    /// <summary>Compara dois nomes sem os espaços das pontas e sem diferenciar maiúsculas.</summary>
    /// <param name="a">Primeiro nome.</param>
    /// <param name="b">Segundo nome.</param>
    /// <returns><see langword="true"/> se forem o mesmo nome para o usuário.</returns>
    public static bool Iguais(string? a, string? b)
    {
        // Comparação em memória: o SQLite só ignora maiúsculas em ASCII ("Água" × "água").
        return string.Equals(a?.Trim(), b?.Trim(), StringComparison.InvariantCultureIgnoreCase);
    }

    /// <summary>Recusa o nome se outra entidade da lista já o usa.</summary>
    /// <param name="existentes">Entidades já cadastradas.</param>
    /// <param name="nome">Nome pretendido.</param>
    /// <param name="ignorarId">Id da própria entidade, numa alteração.</param>
    /// <param name="descricao">Como a entidade é chamada na mensagem, com artigo ("uma conta").</param>
    public static void GarantirUnico(
        IEnumerable<EntidadeNomeada> existentes,
        string nome,
        Guid? ignorarId,
        string descricao)
    {
        if (existentes.Any(e => e.Id != ignorarId && Iguais(e.Nome, nome)))
        {
            throw new RegraNegocioException($"Já existe {descricao} com o nome '{nome.Trim()}'.");
        }
    }

    /// <summary>
    /// Devolve o Id do cadastro com o nome informado, criando-o se ainda não existir. Nome vazio
    /// significa "nenhum".
    /// </summary>
    /// <typeparam name="T">Tipo do cadastro.</typeparam>
    /// <param name="repositorio">Repositório do cadastro.</param>
    /// <param name="nome">Nome digitado pelo usuário.</param>
    /// <param name="criar">Cria o cadastro novo a partir do nome.</param>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <param name="filtro">Restringe a busca (ex.: subcategorias de uma categoria); nulo busca em todos.</param>
    /// <returns>O Id existente ou o do cadastro criado; nulo se o nome estiver vazio.</returns>
    public static async Task<Guid?> ResolverPorNomeAsync<T>(
        IRepositorio<T> repositorio,
        string? nome,
        Func<string, T> criar,
        CancellationToken cancellationToken,
        Func<T, bool>? filtro = null)
        where T : EntidadeNomeada
    {
        if (string.IsNullOrWhiteSpace(nome))
        {
            return null;
        }

        var existente = (await repositorio.ListarAsync(cancellationToken))
            .FirstOrDefault(e => (filtro is null || filtro(e)) && Iguais(e.Nome, nome));
        if (existente is not null)
        {
            return existente.Id;
        }

        var novo = criar(nome);
        repositorio.Adicionar(novo);
        return novo.Id;
    }
}
