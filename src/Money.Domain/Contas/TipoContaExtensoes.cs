// <copyright file="TipoContaExtensoes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Contas;

/// <summary>Comportamentos que dependem do tipo da conta.</summary>
public static class TipoContaExtensoes
{
    /// <summary>Indica se o saldo do tipo é dívida (guardado como negativo).</summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns><see langword="true"/> para Cartão de crédito e Passivo.</returns>
    public static bool RepresentaDivida(this TipoConta tipo)
    {
        return tipo is TipoConta.CartaoCredito or TipoConta.Passivo;
    }

    /// <summary>Indica se o tipo pode ser conta caixa de um investimento.</summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns><see langword="true"/> para Corrente, Poupança e Dinheiro.</returns>
    public static bool PodeSerCaixa(this TipoConta tipo)
    {
        return tipo is TipoConta.Corrente or TipoConta.Poupanca or TipoConta.Dinheiro;
    }
}
