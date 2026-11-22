// <copyright file="Categoria.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Lancamentos;

/// <summary>Categoria de gasto ou recebimento, usada para agrupar os lançamentos nos relatórios.</summary>
public sealed class Categoria : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="Categoria"/>.</summary>
    /// <param name="nome">Nome da categoria.</param>
    public Categoria(string nome)
        : base(nome)
    {
    }

    private Categoria()
    {
    }
}
