// <copyright file="GrupoConta.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Contas;

/// <summary>Grupo criado pelo usuário para organizar contas no gerenciador.</summary>
public sealed class GrupoConta : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="GrupoConta"/>.</summary>
    /// <param name="nome">Nome do grupo.</param>
    public GrupoConta(string nome)
        : base(nome)
    {
    }

    private GrupoConta()
    {
    }
}
