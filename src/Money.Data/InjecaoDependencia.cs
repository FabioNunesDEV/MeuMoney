using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Services.Abstracoes;

namespace Money.Data;

/// <summary>Registro da persistência na injeção de dependência.</summary>
public static class InjecaoDependencia
{
    /// <summary>
    /// Registra a persistência. O provedor de banco é informado por quem chama
    /// (ver Money.Data.Sqlite); este projeto não conhece nenhum provedor específico.
    /// </summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <param name="configurarProvedor">Configura o provedor de banco nas opções do contexto.</param>
    /// <returns>A mesma coleção, para encadear registros.</returns>
    public static IServiceCollection AddMoneyData(
        this IServiceCollection servicos,
        Action<DbContextOptionsBuilder> configurarProvedor)
    {
        servicos.AddDbContext<MoneyDbContext>(configurarProvedor);
        servicos.AddScoped<IUnidadeDeTrabalho>(sp => sp.GetRequiredService<MoneyDbContext>());
        servicos.AddScoped<IInicializadorBanco, InicializadorBanco>();
        return servicos;
    }
}
