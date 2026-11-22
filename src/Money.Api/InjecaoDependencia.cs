// <copyright file="InjecaoDependencia.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Money.Api.Comum;
using Money.Api.Contas;
using Money.Api.Lancamentos;

namespace Money.Api;

/// <summary>Registro e mapeamento da API HTTP local, usada pelas telas.</summary>
public static class InjecaoDependencia
{
    /// <summary>Configura o JSON da API: enums trafegam como texto.</summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <returns>A mesma coleção, para encadear registros.</returns>
    public static IServiceCollection AddMoneyApi(this IServiceCollection servicos)
    {
        servicos.ConfigureHttpJsonOptions(opcoes =>
            opcoes.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        return servicos;
    }

    /// <summary>Mapeia as rotas de todos os módulos sob "/api".</summary>
    /// <param name="rotas">Aplicação ou grupo onde mapear.</param>
    /// <returns>O grupo "/api", para ajustes de quem hospeda.</returns>
    public static RouteGroupBuilder MapMoneyApi(this IEndpointRouteBuilder rotas)
    {
        var api = rotas.MapGroup("/api").AddEndpointFilter<FiltroRegraNegocio>();
        api.MapContas();
        api.MapLancamentos();
        return api;
    }
}
