// <copyright file="ModoContaCaixa.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

namespace Money.Services.Contas;

/// <summary>Como a conta de investimento fica em relação à conta caixa ao salvar.</summary>
public enum ModoContaCaixa
{
    /// <summary>Sem conta caixa.</summary>
    Nenhuma,

    /// <summary>Vincula uma conta já cadastrada.</summary>
    Existente,

    /// <summary>Cria uma conta Dinheiro e a vincula.</summary>
    Nova,
}
