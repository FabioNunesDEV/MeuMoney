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
        new(
            TipoConta.Corrente,
            "Conta corrente",
            "Use para a conta do banco onde entram o salário e saem os pagamentos, saques e transferências do dia a dia."),
        new(
            TipoConta.Poupanca,
            "Poupança",
            "Use para a conta do banco onde você guarda dinheiro rendendo, separado do dia a dia."),
        new(
            TipoConta.Dinheiro,
            "Dinheiro",
            "Use para dinheiro em espécie, como a carteira, e acompanhar os gastos do dia a dia. Também serve de caixa de uma corretora."),
        new(
            TipoConta.CartaoCredito,
            "Cartão de crédito",
            "Use para cada cartão de crédito: registra as compras, e o saldo é o valor devido na fatura."),
        new(
            TipoConta.Investimento,
            "Investimento",
            "Use para ações, fundos e outros ativos numa corretora ou banco; o dinheiro não investido fica numa conta caixa ligada."),
        new(
            TipoConta.Ativo,
            "Ativo",
            "Use para um bem que tem valor de mercado, como um imóvel ou um veículo."),
        new(
            TipoConta.Passivo,
            "Passivo",
            "Use para uma dívida a pagar, como um empréstimo ou um financiamento; o saldo é o valor devido."),
    ];

    /// <summary>Obtém o texto exibido de um tipo.</summary>
    /// <param name="tipo">Tipo da conta.</param>
    /// <returns>Nome e descrição do tipo.</returns>
    public static TipoContaDto Obter(TipoConta tipo)
    {
        return Todos.First(t => t.Tipo == tipo);
    }
}
