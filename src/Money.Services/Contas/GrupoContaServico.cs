// <copyright file="GrupoContaServico.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Services.Abstracoes;
using Money.Services.Comum;

namespace Money.Services.Contas;

/// <summary>Manutenção dos grupos de contas: listar, renomear e excluir.</summary>
/// <param name="grupos">Repositório de grupos.</param>
/// <param name="contas">Repositório de contas, para saber e soltar os membros de cada grupo.</param>
/// <param name="unidadeDeTrabalho">Grava as alterações.</param>
public sealed class GrupoContaServico(
    IRepositorio<GrupoConta> grupos,
    IRepositorio<Conta> contas,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    /// <summary>Lista os grupos com a quantidade de contas de cada um.</summary>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Grupos ordenados por nome.</returns>
    public async Task<IReadOnlyList<CadastroSimplesDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var todasContas = await contas.ListarAsync(cancellationToken);
        return (await grupos.ListarAsync(cancellationToken))
            .OrderBy(g => g.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(g => new CadastroSimplesDto(g.Id, g.Nome, todasContas.Count(c => c.GrupoContaId == g.Id)))
            .ToList();
    }

    /// <summary>Renomeia o grupo; o nome novo vale para todas as contas dele.</summary>
    /// <param name="id">Id do grupo.</param>
    /// <param name="nome">Novo nome, único.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task RenomearAsync(Guid id, string nome, CancellationToken cancellationToken)
    {
        var todos = await grupos.ListarAsync(cancellationToken);
        var grupo = todos.FirstOrDefault(g => g.Id == id)
            ?? throw new RegraNegocioException("Grupo não encontrado.");
        Nomes.GarantirUnico(todos, nome, id, "um grupo");
        grupo.Renomear(nome);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Exclui o grupo; as contas dele ficam sem grupo.</summary>
    /// <param name="id">Id do grupo.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
    {
        var grupo = await grupos.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException("Grupo não encontrado.");
        foreach (var conta in (await contas.ListarAsync(cancellationToken)).Where(c => c.GrupoContaId == id))
        {
            conta.DefinirGrupo(null);
        }

        grupos.Remover(grupo);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }
}
