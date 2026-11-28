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

    /// <summary>
    /// Indica se o tipo tem saldo mínimo no cadastro. Dinheiro não tem: ou o dinheiro existe ou não existe.
    /// </summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns><see langword="true"/> para Corrente e Poupança.</returns>
    public static bool TemSaldoMinimo(this TipoConta tipo)
    {
        return tipo is TipoConta.Corrente or TipoConta.Poupanca;
    }

    /// <summary>
    /// Indica se o assistente de nova conta pergunta o número da conta. Nos Detalhes o número é
    /// editável em todos os tipos.
    /// </summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns><see langword="true"/> para Corrente.</returns>
    public static bool PedeNumeroNaCriacao(this TipoConta tipo)
    {
        return tipo is TipoConta.Corrente;
    }
}
