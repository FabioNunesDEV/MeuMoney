// <copyright file="LancamentoServicoTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;
using Money.Domain.Lancamentos;
using Money.Services.Lancamentos;
using Money.Services.Tests.Fakes;

namespace Money.Services.Tests.Lancamentos;

/// <summary>Testes do serviço de lançamentos.</summary>
public class LancamentoServicoTestes
{
    private static readonly DateOnly Dia = new(2026, 11, 22);

    private readonly LancamentoRepositorioFake lancamentos = new();
    private readonly RepositorioFake<Conta> contas = new();
    private readonly RepositorioFake<Favorecido> favorecidos = new();
    private readonly RepositorioFake<Categoria> categorias = new();
    private readonly RepositorioFake<Subcategoria> subcategorias = new();
    private readonly UnidadeDeTrabalhoFake unidade = new();
    private readonly Conta carteira = new(TipoConta.Dinheiro, "Carteira", "BRL", 100m);
    private readonly Conta cofre = new(TipoConta.Dinheiro, "Cofre", "BRL", 0m);

    /// <summary>Inicia uma nova instância da classe <see cref="LancamentoServicoTestes"/>, com duas contas Dinheiro.</summary>
    public LancamentoServicoTestes()
    {
        this.contas.Adicionar(this.carteira);
        this.contas.Adicionar(this.cofre);
    }

    private LancamentoServico Servico => new(
        this.lancamentos, this.contas, this.favorecidos, this.categorias, this.subcategorias, this.unidade);

    /// <summary>Gasto e recebimento entram na movimentação com o saldo linha a linha.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_GastoERecebimento_MovimentacaoComSaldoPorLinha()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 30m));
        await this.CriarAsync(Recebimento(this.carteira.Id, 50m) with { Data = Dia.AddDays(1) });

        var movimentacao = await this.Servico.ObterMovimentacaoAsync(this.carteira.Id, CancellationToken.None);

        Assert.Equal([70m, 120m], movimentacao.Lancamentos.Select(l => l.SaldoApos));
        Assert.Equal(120m, movimentacao.SaldoFinal);
        Assert.Equal("Padaria", movimentacao.Lancamentos[0].NomeFavorecido);
        Assert.Equal("Alimentação", movimentacao.Lancamentos[0].NomeCategoria);
    }

    /// <summary>Transferência gera uma linha em cada conta, indicando a outra.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_Transferencia_LinhaNasDuasContas()
    {
        await this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 40m));

        var naCarteira = await this.Servico.ObterMovimentacaoAsync(this.carteira.Id, CancellationToken.None);
        var noCofre = await this.Servico.ObterMovimentacaoAsync(this.cofre.Id, CancellationToken.None);

        Assert.Equal(60m, naCarteira.SaldoFinal);
        Assert.Equal(40m, noCofre.SaldoFinal);
        Assert.Equal("Cofre", naCarteira.Lancamentos.Single().NomeOutraConta);
        Assert.Equal("Carteira", noCofre.Lancamentos.Single().NomeOutraConta);
        Assert.Null(naCarteira.Lancamentos.Single().NomeCategoria);
    }

    /// <summary>Gasto sem categoria é recusado; subcategoria é opcional.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_CategoriaObrigatoriaSubcategoriaOpcional()
    {
        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.CriarAsync(Gasto(this.carteira.Id, 10m) with { NomeCategoria = " " }));

        await this.CriarAsync(Gasto(this.carteira.Id, 10m) with { NomeSubcategoria = null });

        Assert.Single(this.lancamentos.Itens);
        Assert.Null(this.lancamentos.Itens[0].SubcategoriaId);
    }

    /// <summary>Nomes já usados são reaproveitados, sem duplicar; subcategoria vale dentro da categoria.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_NomesRepetidos_Reaproveita()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 10m) with { NomeCategoria = "Carro", NomeSubcategoria = "Combustível" });
        await this.CriarAsync(Gasto(this.carteira.Id, 10m) with { NomeFavorecido = " padaria ", NomeCategoria = "carro", NomeSubcategoria = "combustível" });
        await this.CriarAsync(Gasto(this.carteira.Id, 10m) with { NomeCategoria = "Casa", NomeSubcategoria = "Combustível" });

        Assert.Single(this.favorecidos.Itens);
        Assert.Equal(2, this.categorias.Itens.Count);
        Assert.Equal(2, this.subcategorias.Itens.Count);
    }

    /// <summary>As opções trazem a categoria, a subcategoria e o montante do último uso do favorecido.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ObterOpcoesAsync_FavorecidoUsado_TrazUltimoUso()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 70m) with { NomeFavorecido = "Posto", NomeCategoria = "Carro", NomeSubcategoria = "Óleo", Data = Dia.AddDays(-5) });
        await this.CriarAsync(Gasto(this.carteira.Id, 15m) with { NomeFavorecido = "Posto", NomeCategoria = "Carro", NomeSubcategoria = "Combustível" });
        this.contas.Adicionar(new Conta(TipoConta.Dinheiro, "Dólares", "USD", 0m));
        this.contas.Adicionar(new Conta(TipoConta.Corrente, "Banco", "BRL", 0m));

        var opcoes = await this.Servico.ObterOpcoesAsync(this.carteira.Id, CancellationToken.None);

        var posto = opcoes.Favorecidos.Single(f => f.Nome == "Posto");
        Assert.Equal(("Carro", "Combustível", 15m), (posto.UltimaCategoria, posto.UltimaSubcategoria, posto.UltimoValor));
        Assert.Equal(["Combustível", "Óleo"], opcoes.Categorias.Single(c => c.Nome == "Carro").Subcategorias);
        Assert.Equal(["Carteira", "Cofre"], opcoes.Contas.Select(c => c.Nome));
    }

    /// <summary>Alterar uma ponta da transferência altera as duas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AlterarAsync_Transferencia_AlteraAsDuasPontas()
    {
        await this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 40m));
        var entrada = this.lancamentos.Itens.Single(l => l.Tipo == TipoLancamento.TransferenciaEntrada);

        await this.Servico.AlterarAsync(entrada.Id, Transferencia(this.carteira.Id, this.cofre.Id, 25m) with { Memo = "ajuste" }, CancellationToken.None);

        Assert.All(this.lancamentos.Itens, l => Assert.Equal((25m, "ajuste"), (l.Valor, l.Memo)));
    }

    /// <summary>A alteração não muda o tipo do lançamento.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AlterarAsync_OutroTipo_LancaRegraNegocio()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 10m));

        await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.AlterarAsync(this.lancamentos.Itens[0].Id, Recebimento(this.carteira.Id, 10m), CancellationToken.None));
    }

    /// <summary>Excluir uma ponta exclui a transferência inteira.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task ExcluirAsync_Transferencia_ExcluiAsDuasPontas()
    {
        await this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 40m));

        await this.Servico.ExcluirAsync(this.lancamentos.Itens[1].Id, CancellationToken.None);

        Assert.Empty(this.lancamentos.Itens);
    }

    /// <summary>Nulo vale para as duas pontas; C e R só para a marcada; sair de Nulo tira as duas.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MarcarAsync_Transferencia_NuloNasDuasCRSoNaMarcada()
    {
        await this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 40m));
        var saida = this.lancamentos.Itens.Single(l => l.Tipo == TipoLancamento.TransferenciaSaida);
        var entrada = this.lancamentos.Itens.Single(l => l.Tipo == TipoLancamento.TransferenciaEntrada);

        await this.Servico.MarcarAsync(saida.Id, SituacaoLancamento.Compensado, CancellationToken.None);
        Assert.Equal((SituacaoLancamento.Compensado, SituacaoLancamento.NaoReconciliado), (saida.Situacao, entrada.Situacao));

        await this.Servico.MarcarAsync(saida.Id, SituacaoLancamento.Nulo, CancellationToken.None);
        Assert.Equal((SituacaoLancamento.Nulo, SituacaoLancamento.Nulo), (saida.Situacao, entrada.Situacao));

        await this.Servico.MarcarAsync(entrada.Id, SituacaoLancamento.Reconciliado, CancellationToken.None);
        Assert.Equal((SituacaoLancamento.NaoReconciliado, SituacaoLancamento.Reconciliado), (saida.Situacao, entrada.Situacao));
    }

    /// <summary>Mover leva o gasto para outra conta; a transferência não vai para o outro lado.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task MoverAsync_GastoETransferencia()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 30m));
        var gasto = this.lancamentos.Itens[0];
        await this.Servico.MoverAsync(gasto.Id, this.cofre.Id, CancellationToken.None);
        Assert.Equal(this.cofre.Id, gasto.ContaId);

        await this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 10m));
        var saida = this.lancamentos.Itens.Single(l => l.Tipo == TipoLancamento.TransferenciaSaida);
        var erro = await Assert.ThrowsAsync<RegraNegocioException>(
            () => this.Servico.MoverAsync(saida.Id, this.cofre.Id, CancellationToken.None));
        Assert.Equal("A transferência não pode ir para a conta do outro lado.", erro.Message);
    }

    /// <summary>Conta encerrada e conta que não é Dinheiro recusam lançamento.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_ContaQueNaoAceita_LancaRegraNegocio()
    {
        var corrente = new Conta(TipoConta.Corrente, "Banco", "BRL", 0m);
        this.contas.Adicionar(corrente);
        this.cofre.Encerrar();

        await Assert.ThrowsAsync<RegraNegocioException>(() => this.CriarAsync(Gasto(corrente.Id, 10m)));
        await Assert.ThrowsAsync<RegraNegocioException>(() => this.CriarAsync(Gasto(this.cofre.Id, 10m)));
        await Assert.ThrowsAsync<RegraNegocioException>(() => this.CriarAsync(Transferencia(this.carteira.Id, this.cofre.Id, 10m)));
    }

    /// <summary>Sem confirmação, gasto que deixa a conta negativa não grava e devolve o saldo resultante.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_SaldoNegativoSemConfirmacao_NaoGrava()
    {
        var resultado = await this.Servico.CriarAsync(Gasto(this.carteira.Id, 130m), CancellationToken.None);

        Assert.False(resultado.Gravado);
        var negativo = Assert.Single(resultado.SaldosNegativos);
        Assert.Equal(("Carteira", -30m, "BRL"), (negativo.NomeConta, negativo.SaldoFinal, negativo.Moeda.Codigo));
        Assert.Empty(this.lancamentos.Itens);
        Assert.Equal(0, this.unidade.Gravacoes);
    }

    /// <summary>Com confirmação, grava mesmo ficando negativo.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_SaldoNegativoConfirmado_Grava()
    {
        var resultado = await this.Servico.CriarAsync(Gasto(this.carteira.Id, 130m) with { ConfirmarSaldoNegativo = true }, CancellationToken.None);

        Assert.True(resultado.Gravado);
        Assert.Equal(-30m, (await this.Servico.ObterMovimentacaoAsync(this.carteira.Id, CancellationToken.None)).SaldoFinal);
    }

    /// <summary>Recebimento numa conta negativa não pede confirmação; transferência confere só a conta De.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task CriarAsync_RecebimentoEmContaNegativa_NaoPerguntam()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 130m) with { ConfirmarSaldoNegativo = true });

        var recebimento = await this.Servico.CriarAsync(Recebimento(this.carteira.Id, 10m), CancellationToken.None);
        var transferencia = await this.Servico.CriarAsync(Transferencia(this.cofre.Id, this.carteira.Id, 5m), CancellationToken.None);

        Assert.True(recebimento.Gravado);
        Assert.False(transferencia.Gravado);
        Assert.Equal("Cofre", Assert.Single(transferencia.SaldosNegativos).NomeConta);
    }

    /// <summary>Alteração que aumenta o gasto além do saldo pede confirmação, sem gravar.</summary>
    /// <returns>Tarefa do teste.</returns>
    [Fact]
    public async Task AlterarAsync_SaldoNegativoSemConfirmacao_NaoGrava()
    {
        await this.CriarAsync(Gasto(this.carteira.Id, 60m));
        var gravacoes = this.unidade.Gravacoes;

        var resultado = await this.Servico.AlterarAsync(this.lancamentos.Itens[0].Id, Gasto(this.carteira.Id, 120m), CancellationToken.None);

        Assert.False(resultado.Gravado);
        Assert.Equal(-20m, Assert.Single(resultado.SaldosNegativos).SaldoFinal);
        Assert.Equal(gravacoes, this.unidade.Gravacoes);
    }

    private static SalvarLancamentoDto Gasto(Guid contaId, decimal valor) =>
        new(NaturezaLancamento.Gasto, contaId, null, Dia, valor, "Padaria", "Alimentação", "Pão", null, false);

    private static SalvarLancamentoDto Recebimento(Guid contaId, decimal valor) =>
        new(NaturezaLancamento.Recebimento, contaId, null, Dia, valor, "Salário", "Renda", null, null, false);

    private static SalvarLancamentoDto Transferencia(Guid de, Guid para, decimal valor) =>
        new(NaturezaLancamento.Transferencia, de, para, Dia, valor, "Transferência", null, null, null, false);

    private async Task CriarAsync(SalvarLancamentoDto dados)
    {
        var resultado = await this.Servico.CriarAsync(dados, CancellationToken.None);
        Assert.True(resultado.Gravado);
    }
}
