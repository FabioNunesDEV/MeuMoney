using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Services.Abstractions;

namespace Money.Data;

public static class DependencyInjection
{
    /// <summary>
    /// Registra a persistência. O provedor de banco é informado por quem chama
    /// (ver Money.Data.Sqlite); este projeto não conhece nenhum provedor específico.
    /// </summary>
    public static IServiceCollection AddMoneyData(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureProvider)
    {
        services.AddDbContext<MoneyDbContext>(configureProvider);
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<MoneyDbContext>());
        services.AddScoped<IDatabaseInitializer, MoneyDatabaseInitializer>();
        return services;
    }
}
