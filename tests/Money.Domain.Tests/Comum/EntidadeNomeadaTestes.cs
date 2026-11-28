// <copyright file="EntidadeNomeadaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Domain.Comum;

namespace Money.Domain.Tests.Comum;

/// <summary>Testes da validação comum de nome.</summary>
public class EntidadeNomeadaTestes
{
    /// <summary>Nome válido volta sem os espaços das pontas.</summary>
    [Fact]
    public void ValidarNome_ComEspacos_DevolveLimpo()
    {
        Assert.Equal("Conta principal", EntidadeNomeada.ValidarNome("  Conta principal "));
    }

    /// <summary>Nome vazio ou só com espaços é recusado.</summary>
    /// <param name="nome">Nome testado.</param>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidarNome_Vazio_LancaRegraNegocio(string? nome)
    {
        var erro = Assert.Throws<RegraNegocioException>(() => EntidadeNomeada.ValidarNome(nome));

        Assert.Equal("O nome é obrigatório.", erro.Message);
    }

    /// <summary>Nome acima do limite é recusado com a mensagem do limite.</summary>
    [Fact]
    public void ValidarNome_Com101Caracteres_LancaRegraNegocio()
    {
        var erro = Assert.Throws<RegraNegocioException>(() => EntidadeNomeada.ValidarNome(new string('a', 101)));

        Assert.Equal("O nome deve ter no máximo 100 caracteres.", erro.Message);
    }
}
