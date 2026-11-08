using Microsoft.Extensions.DependencyInjection;

namespace Money.Services.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddMoneyServices_ConstroiProviderValidandoEscopos()
    {
        var services = new ServiceCollection().AddMoneyServices();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateScopes = true,
            ValidateOnBuild = true,
        });

        Assert.NotNull(provider);
    }
}
