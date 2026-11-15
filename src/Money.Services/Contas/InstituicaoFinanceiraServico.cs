// <copyright file="InstituicaoFinanceiraServico.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Services.Abstracoes;
using Money.Services.Comum;

namespace Money.Services.Contas;

/// <summary>Manutenção das instituições financeiras: listar, renomear e excluir.</summary>
/// <param name="instituicoes">Repositório de instituições.</param>
/// <param name="contas">Repositório de contas, para saber o uso de cada instituição.</param>
/// <param name="unidadeDeTrabalho">Grava as alterações.</param>
public sealed class InstituicaoFinanceiraServico(
    IRepositorio<InstituicaoFinanceira> instituicoes,
    IRepositorio<Conta> contas,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    /// <summary>Lista as instituições com a quantidade de contas de cada uma.</summary>
    /// <param name="cancellationToken">Cancela a consulta.</param>
    /// <returns>Instituições ordenadas por nome.</returns>
    public async Task<IReadOnlyList<CadastroSimplesDto>> ListarAsync(CancellationToken cancellationToken)
    {
        var todasContas = await contas.ListarAsync(cancellationToken);
        return (await instituicoes.ListarAsync(cancellationToken))
            .OrderBy(i => i.Nome, StringComparer.InvariantCultureIgnoreCase)
            .Select(i => new CadastroSimplesDto(i.Id, i.Nome, todasContas.Count(c => c.InstituicaoFinanceiraId == i.Id)))
            .ToList();
    }

    /// <summary>Renomeia a instituição; o nome novo vale para todas as contas associadas.</summary>
    /// <param name="id">Id da instituição.</param>
    /// <param name="nome">Novo nome, único.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task RenomearAsync(Guid id, string nome, CancellationToken cancellationToken)
    {
        var todas = await instituicoes.ListarAsync(cancellationToken);
        var instituicao = todas.FirstOrDefault(i => i.Id == id)
            ?? throw new RegraNegocioException("Instituição não encontrada.");
        Nomes.GarantirUnico(todas, nome, id, "uma instituição");
        instituicao.Renomear(nome);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }

    /// <summary>Exclui a instituição. Instituição usada por alguma conta não pode ser excluída.</summary>
    /// <param name="id">Id da instituição.</param>
    /// <param name="cancellationToken">Cancela a gravação.</param>
    /// <returns>Tarefa concluída após gravar.</returns>
    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken)
    {
        var instituicao = await instituicoes.ObterPorIdAsync(id, cancellationToken)
            ?? throw new RegraNegocioException("Instituição não encontrada.");
        var emUso = (await contas.ListarAsync(cancellationToken)).Count(c => c.InstituicaoFinanceiraId == id);
        if (emUso > 0)
        {
            throw new RegraNegocioException(
                $"A instituição é usada por {emUso} conta(s) e não pode ser excluída.");
        }

        instituicoes.Remover(instituicao);
        await unidadeDeTrabalho.SalvarAlteracoesAsync(cancellationToken);
    }
}
