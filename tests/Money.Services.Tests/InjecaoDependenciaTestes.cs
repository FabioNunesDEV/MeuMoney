// <copyright file="InjecaoDependenciaTestes.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Money.Services.Abstracoes;
using Money.Services.Contas;
using Money.Services.Tests.Fakes;

namespace Money.Services.Tests;

/// <summary>Testes do registro dos serviços na injeção de dependência.</summary>
public class InjecaoDependenciaTestes
{
    /// <summary>Com a persistência dublada, o provider valida escopos e resolve os serviços.</summary>
    [Fact]
    public void AddMoneyServices_ConstroiProviderValidandoEscopos()
    {
        var servicos = new ServiceCollection().AddMoneyServices();
        servicos.AddScoped(typeof(IRepositorio<>), typeof(RepositorioFake<>));
        servicos.AddScoped<IUnidadeDeTrabalho, UnidadeDeTrabalhoFake>();

        using var provider = servicos.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });
        using var escopo = provider.CreateScope();

        Assert.NotNull(escopo.ServiceProvider.GetRequiredService<ContaServico>());
        Assert.NotNull(escopo.ServiceProvider.GetRequiredService<InstituicaoFinanceiraServico>());
        Assert.NotNull(escopo.ServiceProvider.GetRequiredService<GrupoContaServico>());
    }
}
