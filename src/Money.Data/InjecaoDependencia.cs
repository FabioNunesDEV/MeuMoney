// <copyright file="InjecaoDependencia.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Comum;
using Money.Data.Lancamentos;
using Money.Services.Abstracoes;
using Money.Services.Lancamentos;

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
        servicos.AddScoped(typeof(IRepositorio<>), typeof(Repositorio<>));
        servicos.AddScoped<ILancamentoRepositorio, LancamentoRepositorio>();
        return servicos;
    }
}
