// <copyright file="InjecaoDependenciaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace Money.Services.Tests;

/// <summary>Testes do registro dos serviços na injeção de dependência.</summary>
public class InjecaoDependenciaTestes
{
    /// <summary>O provider montado com os serviços passa na validação de escopos.</summary>
    [Fact]
    public void AddMoneyServices_ConstroiProviderValidandoEscopos()
    {
        var servicos = new ServiceCollection().AddMoneyServices();

        using var provider = servicos.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        Assert.NotNull(provider);
    }
}
