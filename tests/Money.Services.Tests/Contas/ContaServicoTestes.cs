// <copyright file="ContaServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;
using Money.Services.Contas;
using Money.Services.Tests.Fakes;

namespace Money.Services.Tests.Contas;

/// <summary>Testes do serviço de contas.</summary>
public class ContaServicoTestes
{
    private readonly RepositorioFake<Conta> contas = new();
    private readonly RepositorioFake<InstituicaoFinanceira> instituicoes = new();
    private readonly RepositorioFake<GrupoConta> grupos = new();
    private readonly LancamentoRepositorioFake lancamentos = new();
    private readonly UnidadeDeTrabalhoFake unidade = new();

    private ContaServico Servico => new(this.contas, this.instituicoes, this.grupos, this.lancamentos, this.unidade);

    /// <summary>Os sete tipos vêm com nome e descrição.</summary>
    [Fact]
    public void ListarTipos_DevolveSeteTiposComDescricao()
    {
        var tipos = this.Servico.ListarTipos();

        Assert.Equal(Enum.GetValues<TipoConta>().Length, tipos.Count);
        Assert.All(tipos, t => Assert.False(string.IsNullOrWhiteSpace(t.Descricao)));
    }

    /// <summary>Conta válida é gravada e devolvida com os dados.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_DadosValidos_GravaEDevolveConta()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta principal", saldo: 100m), CancellationToken.None);

        Assert.Equal("Conta principal", conta.Nome);
        Assert.Equal(100m, conta.Saldo);
        Assert.Equal("BRL", conta.Moeda.Codigo);
        Assert.Single(this.contas.Itens);
        Assert.Equal(1, this.unidade.Gravacoes);
    }

    /// <summary>Nome igual a outro, mudando só maiúsculas e acento em caixa, é recusado.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_NomeRepetidoOutraCaixa_LancaRegraNegocio()
    {
        await this.Servico.CriarAsync(Dados("Água Viva"), CancellationToken.None);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.CriarAsync(Dados(" água viva "), CancellationToken.None));
    }

    /// <summary>Instituição digitada que não existe é criada e associada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_InstituicaoNova_CriaEAssocia()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta") with { NomeInstituicao = "Banco A" }, CancellationToken.None);

        var instituicao = Assert.Single(this.instituicoes.Itens);
        Assert.Equal("Banco A", conta.NomeInstituicao);
        Assert.Equal(instituicao.Id, conta.InstituicaoFinanceiraId);
    }

    /// <summary>Instituição digitada com outra caixa reaproveita a existente.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_InstituicaoExistenteOutraCaixa_Reaproveita()
    {
        this.instituicoes.Itens.Add(new InstituicaoFinanceira("Banco A"));

        var conta = await this.Servico.CriarAsync(Dados("Conta") with { NomeInstituicao = "banco a" }, CancellationToken.None);

        Assert.Single(this.instituicoes.Itens);
        Assert.Equal("Banco A", conta.NomeInstituicao);
    }

    /// <summary>Grupo novo é criado; grupo vazio deixa a conta sem grupo.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_GrupoNovoEVazio_CriaOuDeixaSemGrupo()
    {
        var comGrupo = await this.Servico.CriarAsync(Dados("Conta 1") with { NomeGrupo = "Exterior" }, CancellationToken.None);
        var semGrupo = await this.Servico.CriarAsync(Dados("Conta 2") with { NomeGrupo = "  " }, CancellationToken.None);

        Assert.Equal("Exterior", comGrupo.NomeGrupo);
        Assert.Null(semGrupo.GrupoContaId);
        Assert.Single(this.grupos.Itens);
    }

    /// <summary>Investimento com caixa nova cria a conta Dinheiro vinculada, na mesma moeda e instituição.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_InvestimentoComCaixaNova_CriaCaixaVinculada()
    {
        var dados = Dados("Corretora", TipoConta.Investimento, "USD") with
        {
            NomeInstituicao = "Corretora X",
            ModoCaixa = ModoContaCaixa.Nova,
            SaldoCaixaNova = 50m,
        };

        var investimento = await this.Servico.CriarAsync(dados, CancellationToken.None);

        var caixa = this.contas.Itens.Single(c => c.Tipo == TipoConta.Dinheiro);
        Assert.Equal("Corretora (Caixa)", caixa.Nome);
        Assert.Equal("USD", caixa.CodigoMoeda);
        Assert.Equal(50m, caixa.SaldoInicial);
        Assert.Equal(investimento.InstituicaoFinanceiraId, caixa.InstituicaoFinanceiraId);
        Assert.Equal(caixa.Id, investimento.ContaCaixaId);
        Assert.Equal("Corretora (Caixa)", investimento.NomeContaCaixa);
    }

    /// <summary>Caixa nova com nome já usado é recusada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_CaixaNovaNomeRepetido_LancaRegraNegocio()
    {
        await this.Servico.CriarAsync(Dados("Corretora (Caixa)", TipoConta.Dinheiro), CancellationToken.None);
        var dados = Dados("Corretora", TipoConta.Investimento) with { ModoCaixa = ModoContaCaixa.Nova };

        await Assert.ThrowsAsync<RegraNegocioException>(() => this.Servico.CriarAsync(dados, CancellationToken.None));
    }

    /// <summary>Caixa existente em outra moeda é recusada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_CaixaExistenteOutraMoeda_LancaRegraNegocio()
    {
        var caixa = await this.Servico.CriarAsync(Dados("Caixa", TipoConta.Dinheiro, "BRL"), CancellationToken.None);
        var dados = Dados("Corretora", TipoConta.Investimento, "USD") with
        {
            ModoCaixa = ModoContaCaixa.Existente,
            ContaCaixaId = caixa.Id,
        };

        await Assert.ThrowsAsync<RegraNegocioException>(() => this.Servico.CriarAsync(dados, CancellationToken.None));
    }

    /// <summary>Só contas abertas, de tipo caixa e na moeda pedida são oferecidas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ListarCaixasCompativeisAsync_FiltraTipoMoedaEAbertas()
    {
        var encerrada = new Conta(TipoConta.Dinheiro, "Encerrada", "USD", 0m);
        encerrada.Encerrar();
        this.contas.Itens.AddRange(
        [
            new Conta(TipoConta.Corrente, "Corrente USD", "USD", 0m),
            new Conta(TipoConta.Dinheiro, "Dinheiro BRL", "BRL", 0m),
            new Conta(TipoConta.CartaoCredito, "Cartão USD", "USD", 0m),
            encerrada,
        ]);

        var caixas = await this.Servico.ListarCaixasCompativeisAsync("USD", CancellationToken.None);

        Assert.Equal("Corrente USD", Assert.Single(caixas).Nome);
    }

    /// <summary>O tipo da conta não muda numa alteração.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AtualizarAsync_TipoDiferente_LancaRegraNegocio()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta"), CancellationToken.None);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.AtualizarAsync(conta.Id, Dados("Conta", TipoConta.Poupanca), CancellationToken.None));
    }

    /// <summary>Alteração válida troca nome e saldo.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AtualizarAsync_DadosValidos_AplicaAlteracoes()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta"), CancellationToken.None);

        var alterada = await this.Servico.AtualizarAsync(conta.Id, Dados("Conta nova", saldo: 25m), CancellationToken.None);

        Assert.Equal("Conta nova", alterada.Nome);
        Assert.Equal(25m, alterada.Saldo);
    }

    /// <summary>Caixa vinculada a investimento não troca de moeda.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AtualizarAsync_MoedaDeCaixaVinculada_LancaRegraNegocio()
    {
        await this.Servico.CriarAsync(
            Dados("Corretora", TipoConta.Investimento, "USD") with { ModoCaixa = ModoContaCaixa.Nova },
            CancellationToken.None);
        var caixa = this.contas.Itens.Single(c => c.Tipo == TipoConta.Dinheiro);

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.AtualizarAsync(caixa.Id, Dados(caixa.Nome, TipoConta.Dinheiro, "BRL"), CancellationToken.None));
    }

    /// <summary>Investimento pode trocar de moeda e de caixa na mesma alteração.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AtualizarAsync_InvestimentoTrocaMoedaECaixa_Aplica()
    {
        var investimento = await this.Servico.CriarAsync(
            Dados("Corretora", TipoConta.Investimento, "USD") with { ModoCaixa = ModoContaCaixa.Nova },
            CancellationToken.None);
        var caixaBrl = await this.Servico.CriarAsync(Dados("Caixa BRL", TipoConta.Dinheiro), CancellationToken.None);

        var alterado = await this.Servico.AtualizarAsync(
            investimento.Id,
            Dados("Corretora", TipoConta.Investimento, "BRL") with { ModoCaixa = ModoContaCaixa.Existente, ContaCaixaId = caixaBrl.Id },
            CancellationToken.None);

        Assert.Equal("BRL", alterado.Moeda.Codigo);
        Assert.Equal(caixaBrl.Id, alterado.ContaCaixaId);
    }

    /// <summary>O total soma por moeda só as contas abertas, com dívida negativa e o real primeiro.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterGerenciadorAsync_TotaisPorMoedaSoAbertas()
    {
        var encerrada = new Conta(TipoConta.Corrente, "Encerrada", "BRL", 1000m);
        encerrada.Encerrar();
        this.contas.Itens.AddRange(
        [
            new Conta(TipoConta.Corrente, "Corrente", "BRL", 100m),
            new Conta(TipoConta.CartaoCredito, "Cartão", "BRL", 30m),
            new Conta(TipoConta.Dinheiro, "Dólares", "USD", 10m),
            encerrada,
        ]);

        var gerenciador = await this.Servico.ObterGerenciadorAsync(false, CancellationToken.None);
        var comEncerradas = await this.Servico.ObterGerenciadorAsync(true, CancellationToken.None);

        Assert.Collection(
            gerenciador.Totais,
            t => Assert.Equal(("BRL", 70m), (t.Moeda.Codigo, t.Total)),
            t => Assert.Equal(("USD", 10m), (t.Moeda.Codigo, t.Total)));
        Assert.Equal(3, gerenciador.Contas.Count);
        Assert.Equal(4, comEncerradas.Contas.Count);
        Assert.Equal(gerenciador.Totais, comEncerradas.Totais);
        Assert.True(gerenciador.ExistemContas);
    }

    /// <summary>Sem contas, o gerenciador informa que não há nenhuma.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterGerenciadorAsync_SemContas_InformaVazio()
    {
        var gerenciador = await this.Servico.ObterGerenciadorAsync(false, CancellationToken.None);

        Assert.False(gerenciador.ExistemContas);
        Assert.Empty(gerenciador.Totais);
    }

    /// <summary>Conta sem vínculo é encerrada e reaberta.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task EncerrarEReabrirAsync_ContaLivre_AlternaEstado()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta"), CancellationToken.None);

        await this.Servico.EncerrarAsync(conta.Id, CancellationToken.None);
        Assert.True(this.contas.Itens.Single().Encerrada);
        await this.Servico.ReabrirAsync(conta.Id, CancellationToken.None);
        Assert.False(this.contas.Itens.Single().Encerrada);
    }

    /// <summary>Caixa de investimento aberto não pode ser encerrada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task EncerrarAsync_CaixaDeInvestimentoAberto_LancaRegraNegocio()
    {
        await this.Servico.CriarAsync(
            Dados("Corretora", TipoConta.Investimento) with { ModoCaixa = ModoContaCaixa.Nova },
            CancellationToken.None);
        var caixa = this.contas.Itens.Single(c => c.Tipo == TipoConta.Dinheiro);

        await Assert.ThrowsAsync<RegraNegocioException>(() => this.Servico.EncerrarAsync(caixa.Id, CancellationToken.None));
    }

    /// <summary>Encerrado o investimento, a conta caixa pode ser encerrada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task EncerrarAsync_CaixaDeInvestimentoEncerrado_Encerra()
    {
        var investimento = await this.Servico.CriarAsync(
            Dados("Corretora", TipoConta.Investimento) with { ModoCaixa = ModoContaCaixa.Nova },
            CancellationToken.None);
        var caixa = this.contas.Itens.Single(c => c.Tipo == TipoConta.Dinheiro);

        await this.Servico.EncerrarAsync(investimento.Id, CancellationToken.None);
        await this.Servico.EncerrarAsync(caixa.Id, CancellationToken.None);

        Assert.All(this.contas.Itens, c => Assert.True(c.Encerrada));
        Assert.Equal(2, this.contas.Itens.Count);
    }

    /// <summary>Favorita é marcada e gravada.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task DefinirFavoritaAsync_Marca()
    {
        var conta = await this.Servico.CriarAsync(Dados("Conta"), CancellationToken.None);

        await this.Servico.DefinirFavoritaAsync(conta.Id, true, CancellationToken.None);

        Assert.True(this.contas.Itens.Single().Favorita);
    }

    /// <summary>Saldo e total do gerenciador somam os lançamentos não nulos ao saldo inicial.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterGerenciador_ComLancamentos_SaldoETotalIncluemLancamentos()
    {
        var criada = await this.Servico.CriarAsync(Dados("Carteira", TipoConta.Dinheiro, saldo: 100m), CancellationToken.None);
        var conta = this.contas.Itens.Single(c => c.Id == criada.Id);
        var anulado = Lancamento.CriarGasto(conta, DadosLancamento(20m));
        anulado.MarcarComo(SituacaoLancamento.Nulo);
        this.lancamentos.Adicionar(Lancamento.CriarRecebimento(conta, DadosLancamento(50m)));
        this.lancamentos.Adicionar(Lancamento.CriarGasto(conta, DadosLancamento(30m)));
        this.lancamentos.Adicionar(anulado);

        var gerenciador = await this.Servico.ObterGerenciadorAsync(false, CancellationToken.None);

        Assert.Equal(120m, gerenciador.Contas.Single().Saldo);
        Assert.Equal(100m, gerenciador.Contas.Single().SaldoInformado);
        Assert.Equal(120m, gerenciador.Totais.Single().Total);
    }

    /// <summary>Conta com lançamentos não troca de moeda.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task Atualizar_MoedaComLancamentos_Recusa()
    {
        var criada = await this.Servico.CriarAsync(Dados("Carteira", TipoConta.Dinheiro), CancellationToken.None);
        var conta = this.contas.Itens.Single(c => c.Id == criada.Id);
        this.lancamentos.Adicionar(Lancamento.CriarRecebimento(conta, DadosLancamento(10m)));

        var erro = await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.AtualizarAsync(conta.Id, Dados("Carteira", TipoConta.Dinheiro, "USD"), CancellationToken.None));

        Assert.Equal("A moeda não muda depois do primeiro lançamento da conta.", erro.Message);
    }

    private static DadosLancamento DadosLancamento(decimal valor) =>
        new(new DateOnly(2026, 11, 22), valor, Guid.CreateVersion7(), Guid.CreateVersion7(), null, null);

    private static SalvarContaDto Dados(
        string nome,
        TipoConta tipo = TipoConta.Corrente,
        string moeda = "BRL",
        decimal saldo = 0m)
    {
        return new SalvarContaDto { Tipo = tipo, Nome = nome, CodigoMoeda = moeda, SaldoInformado = saldo };
    }
}
