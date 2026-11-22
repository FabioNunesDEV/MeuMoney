// <copyright file="InjecaoDependencia.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Money.Services.Contas;
using Money.Services.Lancamentos;

namespace Money.Services;

/// <summary>Registro dos serviços da aplicação na injeção de dependência.</summary>
public static class InjecaoDependencia
{
    /// <summary>Registra os serviços de cada módulo, com o mesmo escopo da persistência.</summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <returns>A mesma coleção, para encadear registros.</returns>
    public static IServiceCollection AddMoneyServices(this IServiceCollection servicos)
    {
        servicos.AddScoped<ContaServico>();
        servicos.AddScoped<InstituicaoFinanceiraServico>();
        servicos.AddScoped<GrupoContaServico>();
        servicos.AddScoped<LancamentoServico>();
        return servicos;
    }
}
