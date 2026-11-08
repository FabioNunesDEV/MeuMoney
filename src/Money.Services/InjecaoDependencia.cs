using Microsoft.Extensions.DependencyInjection;

namespace Money.Services;

/// <summary>Registro dos serviços da aplicação na injeção de dependência.</summary>
public static class InjecaoDependencia
{
    /// <summary>Registra os serviços de cada módulo.</summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <returns>A mesma coleção, para encadear registros.</returns>
    public static IServiceCollection AddMoneyServices(this IServiceCollection servicos)
    {
        // Registrar aqui os serviços de cada módulo.
        return servicos;
    }
}
