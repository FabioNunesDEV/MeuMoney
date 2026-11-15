// <copyright file="Nomes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Services.Comum;

/// <summary>Comparação e unicidade de nomes de cadastro.</summary>
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
}
