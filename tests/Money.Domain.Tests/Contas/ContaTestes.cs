// <copyright file="ContaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Globalization;
using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Domain.Tests.Contas;

/// <summary>Testes das regras da conta.</summary>
public class ContaTestes
{
    /// <summary>Conta nova nasce aberta, sem vínculos e com a moeda normalizada.</summary>
    [Fact]
    public void NovaConta_DadosValidos_NasceAbertaSemVinculos()
    {
        var conta = new Conta(TipoConta.Corrente, " Conta principal ", "brl", 100m);

        Assert.Equal("Conta principal", conta.Nome);
        Assert.Equal("BRL", conta.CodigoMoeda);
        Assert.Equal(100m, conta.SaldoInicial);
        Assert.False(conta.Encerrada);
        Assert.False(conta.Favorita);
        Assert.Null(conta.InstituicaoFinanceiraId);
        Assert.Null(conta.GrupoContaId);
        Assert.Null(conta.ContaCaixaId);
    }

    /// <summary>Tipo fora do enum é recusado.</summary>
    [Fact]
    public void NovaConta_TipoInvalido_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => new Conta((TipoConta)99, "Conta", "BRL", 0m));
    }

    /// <summary>Nome vazio é recusado.</summary>
    [Fact]
    public void NovaConta_NomeVazio_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => new Conta(TipoConta.Corrente, " ", "BRL", 0m));
    }

    /// <summary>Em cartão e passivo, a dívida informada positiva fica negativa.</summary>
    /// <param name="tipo">Tipo de dívida.</param>
    [Theory]
    [InlineData(TipoConta.CartaoCredito)]
    [InlineData(TipoConta.Passivo)]
    public void NovaConta_TipoDivida_GuardaSaldoNegativo(TipoConta tipo)
    {
        var conta = new Conta(tipo, "Dívida", "BRL", 500m);

        Assert.Equal(-500m, conta.SaldoInicial);
        Assert.Equal(500m, conta.SaldoInformado);
    }

    /// <summary>Ativo não aceita valor negativo.</summary>
    [Fact]
    public void NovaConta_AtivoNegativo_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => new Conta(TipoConta.Ativo, "Imóvel", "BRL", -1m));
    }

    /// <summary>Investimento não aceita saldo inicial diferente de zero.</summary>
    [Fact]
    public void NovaConta_InvestimentoComSaldo_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => new Conta(TipoConta.Investimento, "Corretora", "BRL", 10m));
    }

    /// <summary>Valor com mais casas do que a moeda admite é recusado.</summary>
    /// <param name="moeda">Código da moeda.</param>
    /// <param name="valor">Valor com casas demais, em formato invariante.</param>
    [Theory]
    [InlineData("BRL", "10.001")]
    [InlineData("JPY", "10.5")]
    public void NovaConta_CasasDecimaisDemais_LancaRegraNegocio(string moeda, string valor)
    {
        var saldo = decimal.Parse(valor, CultureInfo.InvariantCulture);

        Assert.Throws<RegraNegocioException>(() => new Conta(TipoConta.Corrente, "Conta", moeda, saldo));
    }

    /// <summary>Saldo que não cabe nas casas da nova moeda é recusado e nada muda.</summary>
    [Fact]
    public void DefinirMoedaESaldo_SaldoNaoCabe_LancaRegraNegocioSemAlterar()
    {
        var conta = new Conta(TipoConta.Corrente, "Conta", "USD", 10.5m);

        Assert.Throws<RegraNegocioException>(() => conta.DefinirMoedaESaldo("JPY", 10.5m));
        Assert.Equal("USD", conta.CodigoMoeda);
        Assert.Equal(10.5m, conta.SaldoInicial);
    }

    /// <summary>Moeda e saldo trocados juntos são validados na nova moeda.</summary>
    [Fact]
    public void DefinirMoedaESaldo_MoedaESaldoJuntos_Aplica()
    {
        var conta = new Conta(TipoConta.Corrente, "Conta", "USD", 10.5m);

        conta.DefinirMoedaESaldo("JPY", 10m);

        Assert.Equal("JPY", conta.CodigoMoeda);
        Assert.Equal(10m, conta.SaldoInicial);
    }

    /// <summary>Investimento com caixa vinculada não troca de moeda.</summary>
    [Fact]
    public void DefinirMoedaESaldo_InvestimentoComCaixa_LancaRegraNegocio()
    {
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "USD", 0m);
        investimento.VincularCaixa(new Conta(TipoConta.Dinheiro, "Caixa", "USD", 0m));

        Assert.Throws<RegraNegocioException>(() => investimento.DefinirMoedaESaldo("BRL", 0m));
    }

    /// <summary>Caixa compatível fica vinculada ao investimento.</summary>
    [Fact]
    public void VincularCaixa_Compativel_GuardaVinculo()
    {
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "USD", 0m);
        var caixa = new Conta(TipoConta.Dinheiro, "Corretora (Caixa)", "USD", 50m);

        investimento.VincularCaixa(caixa);

        Assert.Equal(caixa.Id, investimento.ContaCaixaId);
    }

    /// <summary>Caixa em outra moeda é recusada.</summary>
    [Fact]
    public void VincularCaixa_OutraMoeda_LancaRegraNegocio()
    {
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "USD", 0m);
        var caixa = new Conta(TipoConta.Dinheiro, "Caixa", "BRL", 0m);

        Assert.Throws<RegraNegocioException>(() => investimento.VincularCaixa(caixa));
    }

    /// <summary>Caixa de tipo incompatível é recusada.</summary>
    [Fact]
    public void VincularCaixa_TipoIncompativel_LancaRegraNegocio()
    {
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "BRL", 0m);
        var cartao = new Conta(TipoConta.CartaoCredito, "Cartão", "BRL", 0m);

        Assert.Throws<RegraNegocioException>(() => investimento.VincularCaixa(cartao));
    }

    /// <summary>Caixa encerrada é recusada.</summary>
    [Fact]
    public void VincularCaixa_CaixaEncerrada_LancaRegraNegocio()
    {
        var investimento = new Conta(TipoConta.Investimento, "Corretora", "BRL", 0m);
        var caixa = new Conta(TipoConta.Dinheiro, "Caixa", "BRL", 0m);
        caixa.Encerrar();

        Assert.Throws<RegraNegocioException>(() => investimento.VincularCaixa(caixa));
    }

    /// <summary>Conta que não é investimento não recebe caixa.</summary>
    [Fact]
    public void VincularCaixa_ContaNaoInvestimento_LancaRegraNegocio()
    {
        var corrente = new Conta(TipoConta.Corrente, "Conta", "BRL", 0m);
        var caixa = new Conta(TipoConta.Dinheiro, "Caixa", "BRL", 0m);

        Assert.Throws<RegraNegocioException>(() => corrente.VincularCaixa(caixa));
    }

    /// <summary>Encerrar e reabrir alternam o estado; favorita liga e desliga.</summary>
    [Fact]
    public void EncerrarReabrirFavorita_AlternamEstado()
    {
        var conta = new Conta(TipoConta.Corrente, "Conta", "BRL", 0m);

        conta.Encerrar();
        Assert.True(conta.Encerrada);
        conta.Reabrir();
        Assert.False(conta.Encerrada);
        conta.DefinirFavorita(true);
        Assert.True(conta.Favorita);
    }
}
