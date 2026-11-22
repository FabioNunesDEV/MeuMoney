// <copyright file="TipoLancamento.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Lancamentos;

/// <summary>Natureza do lançamento: diz se ele tira ou põe dinheiro na conta.</summary>
public enum TipoLancamento
{
    /// <summary>Saída de dinheiro (coluna Gastar).</summary>
    Gasto,

    /// <summary>Entrada de dinheiro (coluna Receber).</summary>
    Recebimento,

    /// <summary>Ponta de saída de uma transferência entre contas.</summary>
    TransferenciaSaida,

    /// <summary>Ponta de entrada de uma transferência entre contas.</summary>
    TransferenciaEntrada,
}
