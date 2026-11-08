using Microsoft.Extensions.DependencyInjection;

namespace Money.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddMoneyServices(this IServiceCollection services)
    {
        // Registrar aqui os casos de uso de cada módulo.
        return services;
    }
}
