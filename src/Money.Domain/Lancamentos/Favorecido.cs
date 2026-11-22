// <copyright file="Favorecido.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Lancamentos;

/// <summary>A quem se paga ou de quem se recebe ("Pagar a" no lançamento).</summary>
public sealed class Favorecido : EntidadeNomeada
{
    /// <summary>Inicia uma nova instância da classe <see cref="Favorecido"/>.</summary>
    /// <param name="nome">Nome do favorecido.</param>
    public Favorecido(string nome)
        : base(nome)
    {
    }

    private Favorecido()
    {
    }
}
