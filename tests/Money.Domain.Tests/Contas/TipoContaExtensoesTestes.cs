// <copyright file="TipoContaExtensoesTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Contas;

namespace Money.Domain.Tests.Contas;

/// <summary>Testes dos comportamentos por tipo de conta.</summary>
public class TipoContaExtensoesTestes
{
    /// <summary>Só cartão de crédito e passivo representam dívida.</summary>
    /// <param name="tipo">Tipo testado.</param>
    /// <param name="esperado">Resultado esperado.</param>
    [Theory]
    [InlineData(TipoConta.Corrente, false)]
    [InlineData(TipoConta.Poupanca, false)]
    [InlineData(TipoConta.Dinheiro, false)]
    [InlineData(TipoConta.CartaoCredito, true)]
    [InlineData(TipoConta.Investimento, false)]
    [InlineData(TipoConta.Ativo, false)]
    [InlineData(TipoConta.Passivo, true)]
    public void RepresentaDivida_PorTipo_DevolveEsperado(TipoConta tipo, bool esperado)
    {
        Assert.Equal(esperado, tipo.RepresentaDivida());
    }

    /// <summary>Só corrente, poupança e dinheiro podem ser conta caixa.</summary>
    /// <param name="tipo">Tipo testado.</param>
    /// <param name="esperado">Resultado esperado.</param>
    [Theory]
    [InlineData(TipoConta.Corrente, true)]
    [InlineData(TipoConta.Poupanca, true)]
    [InlineData(TipoConta.Dinheiro, true)]
    [InlineData(TipoConta.CartaoCredito, false)]
    [InlineData(TipoConta.Investimento, false)]
    [InlineData(TipoConta.Ativo, false)]
    [InlineData(TipoConta.Passivo, false)]
    public void PodeSerCaixa_PorTipo_DevolveEsperado(TipoConta tipo, bool esperado)
    {
        Assert.Equal(esperado, tipo.PodeSerCaixa());
    }
}
