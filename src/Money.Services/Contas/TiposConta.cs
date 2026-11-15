// <copyright file="TiposConta.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Contas;

namespace Money.Services.Contas;

/// <summary>Nome e descrição exibidos de cada tipo de conta.</summary>
internal static class TiposConta
{
    /// <summary>Obtém todos os tipos, na ordem de exibição.</summary>
    public static IReadOnlyList<TipoContaDto> Todos { get; } =
    [
        new(TipoConta.Corrente, "Conta corrente", "Conta bancária do dia a dia: depósitos, pagamentos e transferências."),
        new(TipoConta.Poupanca, "Poupança", "Conta bancária para guardar dinheiro com rendimento."),
        new(TipoConta.Dinheiro, "Dinheiro", "Dinheiro em espécie ou caixa de corretora, onde fica o valor não investido."),
        new(TipoConta.CartaoCredito, "Cartão de crédito", "Compras no cartão; o saldo é o valor devido."),
        new(TipoConta.Investimento, "Investimento", "Ações, fundos e outros ativos numa corretora ou banco, com uma conta caixa ligada."),
        new(TipoConta.Ativo, "Ativo", "Bem com valor de mercado, como imóvel ou veículo."),
        new(TipoConta.Passivo, "Passivo", "Dívida a pagar, como empréstimo ou financiamento."),
    ];

    /// <summary>Obtém o texto exibido de um tipo.</summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns>Nome e descrição do tipo.</returns>
    public static TipoContaDto Obter(TipoConta tipo)
    {
        return Todos.First(t => t.Tipo == tipo);
    }
}
