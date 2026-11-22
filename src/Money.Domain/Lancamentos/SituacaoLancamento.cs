// <copyright file="SituacaoLancamento.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Domain.Lancamentos;

/// <summary>Situação do lançamento, como na coluna C do Money.</summary>
public enum SituacaoLancamento
{
    /// <summary>Ainda não conferido (padrão).</summary>
    NaoReconciliado,

    /// <summary>Já aconteceu ou foi compensado (C).</summary>
    Compensado,

    /// <summary>Conferido com o extrato (R).</summary>
    Reconciliado,

    /// <summary>Anulado: continua registrado, mas não conta no saldo.</summary>
    Nulo,
}
