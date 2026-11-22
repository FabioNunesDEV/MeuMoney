// <copyright file="LancamentosEndpoints.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Money.Services.Lancamentos;

namespace Money.Api.Lancamentos;

/// <summary>Rotas do módulo Lançamentos: movimentação da conta e operações sobre os lançamentos.</summary>
internal static class LancamentosEndpoints
{
    /// <summary>Mapeia as rotas do módulo sob o grupo informado.</summary>
    /// <param name="api">Grupo "/api".</param>
    public static void MapLancamentos(this RouteGroupBuilder api)
    {
        api.MapGet("/contas/{id:guid}/lancamentos", (Guid id, LancamentoServico servico, CancellationToken ct) =>
            servico.ObterMovimentacaoAsync(id, ct));

        var lancamentos = api.MapGroup("/lancamentos");
        lancamentos.MapGet("/opcoes", (Guid contaId, LancamentoServico servico, CancellationToken ct) =>
            servico.ObterOpcoesAsync(contaId, ct));
        lancamentos.MapPost("/", (SalvarLancamentoDto dados, LancamentoServico servico, CancellationToken ct) =>
            servico.CriarAsync(dados, ct));
        lancamentos.MapPut("/{id:guid}", (Guid id, SalvarLancamentoDto dados, LancamentoServico servico, CancellationToken ct) =>
            servico.AlterarAsync(id, dados, ct));
        lancamentos.MapPut("/{id:guid}/situacao", (Guid id, SituacaoDto dados, LancamentoServico servico, CancellationToken ct) =>
            servico.MarcarAsync(id, dados.Situacao, ct));
        lancamentos.MapPost("/{id:guid}/mover", (Guid id, MoverLancamentoDto dados, LancamentoServico servico, CancellationToken ct) =>
            servico.MoverAsync(id, dados.ContaId, ct));
        lancamentos.MapDelete("/{id:guid}", (Guid id, LancamentoServico servico, CancellationToken ct) =>
            servico.ExcluirAsync(id, ct));
    }
}
