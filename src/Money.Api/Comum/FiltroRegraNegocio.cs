// <copyright file="FiltroRegraNegocio.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.AspNetCore.Http;
using Money.Domain.Comum;

namespace Money.Api.Comum;

/// <summary>
/// Converte uma regra de negócio violada em resposta 400 com a mensagem pronta para a tela exibir.
/// </summary>
internal sealed class FiltroRegraNegocio : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (RegraNegocioException ex)
        {
            return Results.BadRequest(new ErroDto(ex.Message));
        }
    }
}
