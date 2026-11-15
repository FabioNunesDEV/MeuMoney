// <copyright file="CadastrosSimplesServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Services.Contas;
using Money.Services.Tests.Fakes;

namespace Money.Services.Tests.Contas;

/// <summary>Testes dos serviços de instituições financeiras e grupos de contas.</summary>
public class CadastrosSimplesServicoTestes
{
    private readonly RepositorioFake<Conta> contas = new();
    private readonly RepositorioFake<InstituicaoFinanceira> instituicoes = new();
    private readonly RepositorioFake<GrupoConta> grupos = new();
    private readonly UnidadeDeTrabalhoFake unidade = new();

    private InstituicaoFinanceiraServico ServicoInstituicoes => new(this.instituicoes, this.contas, this.unidade);

    private GrupoContaServico ServicoGrupos => new(this.grupos, this.contas, this.unidade);

    /// <summary>A lista traz a quantidade de contas, encerradas incluídas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListarInstituicoesAsync_ContaUsoComEncerradas()
    {
        var instituicao = new InstituicaoFinanceira("Banco A");
        this.instituicoes.Itens.Add(instituicao);
        this.contas.Itens.Add(this.Conta("Conta 1", instituicaoId: instituicao.Id));
        var encerrada = this.Conta("Conta 2", instituicaoId: instituicao.Id);
        encerrada.Encerrar();
        this.contas.Itens.Add(encerrada);

        var lista = await this.ServicoInstituicoes.ListarAsync(CancellationToken.None);

        Assert.Equal(2, Assert.Single(lista).QuantidadeContas);
    }

    /// <summary>Renomear para o nome de outra instituição é recusado.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RenomearInstituicaoAsync_NomeExistente_LancaRegraNegocio()
    {
        var banco = new InstituicaoFinanceira("Banco A");
        this.instituicoes.Itens.AddRange([banco, new InstituicaoFinanceira("Banco B")]);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.ServicoInstituicoes.RenomearAsync(banco.Id, "banco b", CancellationToken.None));
    }

    /// <summary>Instituição em uso não é excluída; sem uso, é.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExcluirInstituicaoAsync_BloqueiaEmUsoEExcluiSemUso()
    {
        var emUso = new InstituicaoFinanceira("Banco A");
        var livre = new InstituicaoFinanceira("Banco B");
        this.instituicoes.Itens.AddRange([emUso, livre]);
        this.contas.Itens.Add(this.Conta("Conta", instituicaoId: emUso.Id));

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.ServicoInstituicoes.ExcluirAsync(emUso.Id, CancellationToken.None));
        await this.ServicoInstituicoes.ExcluirAsync(livre.Id, CancellationToken.None);

        Assert.Equal(emUso.Id, Assert.Single(this.instituicoes.Itens).Id);
    }

    /// <summary>Renomear grupo troca o nome.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task RenomearGrupoAsync_NomeValido_TrocaNome()
    {
        var grupo = new GrupoConta("Exterior");
        this.grupos.Itens.Add(grupo);

        await this.ServicoGrupos.RenomearAsync(grupo.Id, "Internacional", CancellationToken.None);

        Assert.Equal("Internacional", grupo.Nome);
    }

    /// <summary>Excluir grupo em uso deixa as contas sem grupo.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExcluirGrupoAsync_EmUso_SoltaContas()
    {
        var grupo = new GrupoConta("Exterior");
        this.grupos.Itens.Add(grupo);
        var conta = this.Conta("Conta", grupoId: grupo.Id);
        this.contas.Itens.Add(conta);

        await this.ServicoGrupos.ExcluirAsync(grupo.Id, CancellationToken.None);

        Assert.Empty(this.grupos.Itens);
        Assert.Null(conta.GrupoContaId);
    }

    private Conta Conta(string nome, Guid? instituicaoId = null, Guid? grupoId = null)
    {
        var conta = new Conta(TipoConta.Corrente, nome, "BRL", 0m);
        conta.DefinirInstituicao(instituicaoId);
        conta.DefinirGrupo(grupoId);
        return conta;
    }
}
