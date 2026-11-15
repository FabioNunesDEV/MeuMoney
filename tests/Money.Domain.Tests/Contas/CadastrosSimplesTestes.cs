// <copyright file="CadastrosSimplesTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;
using Money.Domain.Contas;

namespace Money.Domain.Tests.Contas;

/// <summary>Testes de instituição financeira e grupo de contas.</summary>
public class CadastrosSimplesTestes
{
    /// <summary>O nome é guardado sem os espaços das pontas.</summary>
    [Fact]
    public void NovaInstituicao_NomeComEspacos_GuardaSemEspacos()
    {
        var instituicao = new InstituicaoFinanceira("  Banco A  ");

        Assert.Equal("Banco A", instituicao.Nome);
    }

    /// <summary>Nome vazio ou só com espaços é recusado.</summary>
    /// <param name="nome">Nome testado.</param>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void NovoGrupo_NomeVazio_LancaRegraNegocio(string nome)
    {
        Assert.Throws<RegraNegocioException>(() => new GrupoConta(nome));
    }

    /// <summary>Nome acima de 100 caracteres é recusado ao renomear.</summary>
    [Fact]
    public void RenomearGrupo_NomeLongo_LancaRegraNegocio()
    {
        var grupo = new GrupoConta("Exterior");

        Assert.Throws<RegraNegocioException>(() => grupo.Renomear(new string('a', 101)));
    }

    /// <summary>Renomear troca o nome da instituição.</summary>
    [Fact]
    public void RenomearInstituicao_NomeValido_TrocaNome()
    {
        var instituicao = new InstituicaoFinanceira("Banco A");

        instituicao.Renomear("Banco A S.A.");

        Assert.Equal("Banco A S.A.", instituicao.Nome);
    }
}
