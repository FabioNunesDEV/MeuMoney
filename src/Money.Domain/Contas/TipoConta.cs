// <copyright file="TipoConta.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Contas;

/// <summary>Tipo da conta. Cada tipo tem comportamento fixo; o usuário não cria tipos.</summary>
public enum TipoConta
{
    /// <summary>Conta corrente bancária.</summary>
    Corrente,

    /// <summary>Conta poupança.</summary>
    Poupanca,

    /// <summary>Dinheiro em espécie ou caixa de corretora.</summary>
    Dinheiro,

    /// <summary>Cartão de crédito; o saldo é dívida.</summary>
    CartaoCredito,

    /// <summary>Investimentos mantidos numa corretora ou banco.</summary>
    Investimento,

    /// <summary>Bem com valor, como imóvel ou veículo.</summary>
    Ativo,

    /// <summary>Dívida, como empréstimo ou financiamento.</summary>
    Passivo,
}
