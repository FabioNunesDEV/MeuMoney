using Microsoft.Extensions.DependencyInjection;

namespace Money.Services.Tests;

public class InjecaoDependenciaTestes
{
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
