// <copyright file="NaturezaLancamento.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Lancamentos;

/// <summary>O que o usuário está registrando: a aba do formulário de lançamento.</summary>
public enum NaturezaLancamento
{
    /// <summary>Saída de dinheiro da conta.</summary>
    Gasto,

    /// <summary>Entrada de dinheiro na conta.</summary>
    Recebimento,

    /// <summary>Saída de uma conta (De) e entrada em outra (Para).</summary>
    Transferencia,
}
