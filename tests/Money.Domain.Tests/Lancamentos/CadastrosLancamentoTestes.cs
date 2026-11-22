// <copyright file="CadastrosLancamentoTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Lancamentos;

namespace Money.Domain.Tests.Lancamentos;

/// <summary>Testes de favorecido, categoria e subcategoria.</summary>
public class CadastrosLancamentoTestes
{
    /// <summary>Favorecido guarda o nome sem os espaços das pontas.</summary>
    [Fact]
    public void NovoFavorecido_NomeComEspacos_GuardaSemEspacos()
    {
        Assert.Equal("Padaria", new Favorecido("  Padaria ").Nome);
    }

    /// <summary>Categoria sem nome é recusada.</summary>
    [Fact]
    public void NovaCategoria_NomeVazio_LancaRegraNegocio()
    {
        Assert.Throws<RegraNegocioException>(() => new Categoria(" "));
    }

    /// <summary>Subcategoria guarda a categoria a que pertence.</summary>
    [Fact]
    public void NovaSubcategoria_GuardaCategoria()
    {
        var categoria = new Categoria("Carro");

        var subcategoria = new Subcategoria(categoria.Id, "Combustível");

        Assert.Equal(categoria.Id, subcategoria.CategoriaId);
        Assert.Equal("Combustível", subcategoria.Nome);
    }
}
