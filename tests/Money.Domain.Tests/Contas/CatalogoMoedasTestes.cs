// <copyright file="CatalogoMoedasTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Domain.Tests.Contas;

/// <summary>Testes do catálogo de moedas.</summary>
public class CatalogoMoedasTestes
{
    /// <summary>A busca encontra a moeda sem diferenciar maiúsculas.</summary>
    [Fact]
    public void Obter_CodigoMinusculo_DevolveMoeda()
    {
        var moeda = CatalogoMoedas.Obter("usd");

        Assert.Equal("USD", moeda.Codigo);
        Assert.Equal("US$", moeda.Simbolo);
    }

    /// <summary>Código fora do catálogo é recusado.</summary>
    [Fact]
    public void Obter_CodigoInexistente_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => CatalogoMoedas.Obter("XYZ"));
    }

    /// <summary>O catálogo traz ao menos real, dólar e euro, e o padrão é o real.</summary>
    [Fact]
    public void Todas_ContemMoedasObrigatorias()
    {
        var codigos = CatalogoMoedas.Todas.Select(m => m.Codigo).ToList();

        Assert.Contains("BRL", codigos);
        Assert.Contains("USD", codigos);
        Assert.Contains("EUR", codigos);
        Assert.Equal("BRL", CatalogoMoedas.CodigoPadrao);
    }
}
