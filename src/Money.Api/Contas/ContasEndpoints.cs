// <copyright file="ContasEndpoints.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Money.Api.Comum;
using Money.Services.Contas;

namespace Money.Api.Contas;

/// <summary>Rotas do módulo Contas: contas, instituições financeiras e grupos.</summary>
internal static class ContasEndpoints
{
    /// <summary>Mapeia as rotas do módulo sob o grupo informado.</summary>
    /// <param name="api">Grupo "/api".</param>
    public static void MapContas(this RouteGroupBuilder api)
    {
        var contas = api.MapGroup("/contas");
        contas.MapGet("/", (bool? encerradas, ContaServico servico, CancellationToken ct) =>
            servico.ObterGerenciadorAsync(encerradas ?? false, ct));
        contas.MapGet("/opcoes", (ContaServico servico) =>
            new OpcoesContaDto(servico.ListarTipos(), servico.ListarMoedas()));
        contas.MapGet("/caixas", (string moeda, ContaServico servico, CancellationToken ct) =>
            servico.ListarCaixasCompativeisAsync(moeda, ct));
        contas.MapGet("/{id:guid}", (Guid id, ContaServico servico, CancellationToken ct) =>
            servico.ObterAsync(id, ct));
        contas.MapPost("/", (SalvarContaDto dados, ContaServico servico, CancellationToken ct) =>
            servico.CriarAsync(dados, ct));
        contas.MapPut("/{id:guid}", (Guid id, SalvarContaDto dados, ContaServico servico, CancellationToken ct) =>
            servico.AtualizarAsync(id, dados, ct));
        contas.MapPut("/{id:guid}/favorita", (Guid id, FavoritaDto dados, ContaServico servico, CancellationToken ct) =>
            servico.DefinirFavoritaAsync(id, dados.Favorita, ct));
        contas.MapPost("/{id:guid}/encerrar", (Guid id, ContaServico servico, CancellationToken ct) =>
            servico.EncerrarAsync(id, ct));
        contas.MapPost("/{id:guid}/reabrir", (Guid id, ContaServico servico, CancellationToken ct) =>
            servico.ReabrirAsync(id, ct));

        var instituicoes = api.MapGroup("/instituicoes");
        instituicoes.MapGet("/", (InstituicaoFinanceiraServico servico, CancellationToken ct) =>
            servico.ListarAsync(ct));
        instituicoes.MapPut("/{id:guid}", (Guid id, NomeDto dados, InstituicaoFinanceiraServico servico, CancellationToken ct) =>
            servico.RenomearAsync(id, dados.Nome, ct));
        instituicoes.MapDelete("/{id:guid}", (Guid id, InstituicaoFinanceiraServico servico, CancellationToken ct) =>
            servico.ExcluirAsync(id, ct));

        var grupos = api.MapGroup("/grupos");
        grupos.MapGet("/", (GrupoContaServico servico, CancellationToken ct) =>
            servico.ListarAsync(ct));
        grupos.MapPut("/{id:guid}", (Guid id, NomeDto dados, GrupoContaServico servico, CancellationToken ct) =>
            servico.RenomearAsync(id, dados.Nome, ct));
        grupos.MapDelete("/{id:guid}", (Guid id, GrupoContaServico servico, CancellationToken ct) =>
            servico.ExcluirAsync(id, ct));
    }
}
