// <copyright file="TesteDeApi.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Money.Data.Sqlite;
using Money.Services;
using Money.Services.Abstracoes;

namespace Money.Api.Tests;

/// <summary>
/// Base dos testes de ponta a ponta: sobe a API em porta livre, com SQLite num arquivo temporário
/// novo para cada teste.
/// </summary>
public abstract class TesteDeApi : IAsyncLifetime, IDisposable
{
    /// <summary>Opções de JSON iguais às da API: enums como texto.</summary>
    protected static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly string pasta = Path.Combine(Path.GetTempPath(), "MeuMoneyTests", Guid.NewGuid().ToString("N"));
    private WebApplication? aplicacao;
    private HttpClient? cliente;

    /// <summary>Obtém o cliente HTTP apontado para a API.</summary>
    protected HttpClient Cliente => this.cliente ?? throw new InvalidOperationException("API não iniciada.");

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Services.AddMoneyServices();
        builder.Services.AddMoneySqlite($"Data Source={Path.Combine(this.pasta, "teste.db")}");
        builder.Services.AddMoneyApi();
        this.aplicacao = builder.Build();
        this.aplicacao.MapMoneyApi();

        await using (var escopo = this.aplicacao.Services.CreateAsyncScope())
        {
            await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();
        }

        await this.aplicacao.StartAsync();
        var endereco = this.aplicacao.Services.GetRequiredService<IServer>()
            .Features.Get<IServerAddressesFeature>()?.Addresses.First();
        this.cliente = new HttpClient { BaseAddress = new Uri(endereco ?? throw new InvalidOperationException("Sem endereço.")) };
    }

    /// <inheritdoc />
    public async Task DisposeAsync()
    {
        this.cliente?.Dispose();
        if (this.aplicacao is not null)
        {
            await this.aplicacao.DisposeAsync();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        try
        {
            Directory.Delete(this.pasta, recursive: true);
        }
        catch (IOException)
        {
            // Limpeza best-effort de arquivo temporário.
        }
    }
}
