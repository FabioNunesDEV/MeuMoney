// <copyright file="CatalogoMoedas.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Contas;

/// <summary>Moedas aceitas nas contas. Moeda nova entra como mais uma linha da lista.</summary>
public static class CatalogoMoedas
{
    /// <summary>Código da moeda usada quando o usuário não escolhe outra.</summary>
    public const string CodigoPadrao = "BRL";

    private static readonly Moeda[] Moedas =
    [
        new("BRL", "Real brasileiro", "R$", 2),
        new("USD", "Dólar americano", "US$", 2),
        new("EUR", "Euro", "€", 2),
        new("GBP", "Libra esterlina", "£", 2),
        new("CHF", "Franco suíço", "CHF", 2),
        new("JPY", "Iene japonês", "¥", 0),
        new("CAD", "Dólar canadense", "C$", 2),
        new("AUD", "Dólar australiano", "A$", 2),
        new("ARS", "Peso argentino", "AR$", 2),
        new("CLP", "Peso chileno", "CLP$", 0),
        new("MXN", "Peso mexicano", "MX$", 2),
        new("CNY", "Yuan chinês", "CN¥", 2),
    ];

    /// <summary>Obtém todas as moedas aceitas.</summary>
    public static IReadOnlyList<Moeda> Todas => Moedas;

    /// <summary>Obtém a moeda pelo código, sem diferenciar maiúsculas.</summary>
    /// <param name="codigo">Código ISO 4217.</param>
    /// <returns>A moeda correspondente.</returns>
    public static Moeda Obter(string? codigo)
    {
        return Moedas.FirstOrDefault(m => string.Equals(m.Codigo, codigo, StringComparison.OrdinalIgnoreCase))
            ?? throw new RegraNegocioException($"Moeda não suportada: '{codigo}'.");
    }
}
