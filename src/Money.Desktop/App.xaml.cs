// <copyright file="App.xaml.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.IO;
using System.Windows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Money.Api;
using Money.Data.Sqlite;
using Money.Services;
using Money.Services.Abstracoes;

namespace Money.Desktop;

/// <summary>
/// Composition root: sobe o servidor local (API e telas), prepara o banco e abre a janela.
/// </summary>
public partial class App : Application
{
    private WebApplication? servidor;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                ContentRootPath = AppContext.BaseDirectory,
                WebRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot"),
            });

            // Só a própria máquina alcança o servidor; porta 0 pede uma porta livre ao sistema.
            builder.WebHost.UseUrls($"http://127.0.0.1:{builder.Configuration.GetValue("Porta", 0)}");
            builder.Services.AddMoneyServices();
            AdicionarBanco(builder.Services, builder.Configuration);
            builder.Services.AddMoneyApi();

            this.servidor = builder.Build();
            this.servidor.UseDefaultFiles();
            this.servidor.UseStaticFiles();
            this.servidor.MapMoneyApi();

            await using (var escopo = this.servidor.Services.CreateAsyncScope())
            {
                await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();
            }

            await this.servidor.StartAsync();
            var endereco = this.servidor.Services.GetRequiredService<IServer>()
                .Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault()
                ?? throw new InvalidOperationException("O servidor local não informou o endereço.");

            this.MainWindow = new JanelaPrincipal(new Uri(endereco));
            this.MainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Meu Money", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Shutdown(1);
        }
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        if (this.servidor is not null)
        {
            await this.servidor.StopAsync();
            await this.servidor.DisposeAsync();
        }

        base.OnExit(e);
    }

    /// <summary>
    /// Único ponto que escolhe o provedor de banco. Para usar SQL Server, referenciar
    /// Money.Data.SqlServer e adicionar o caso correspondente aqui.
    /// </summary>
    /// <param name="servicos">Coleção de serviços do host.</param>
    /// <param name="configuracao">Configuração lida do appsettings.json (seção Database).</param>
    private static void AdicionarBanco(IServiceCollection servicos, IConfiguration configuracao)
    {
        var provedor = configuracao["Database:Provider"];
        var stringConexao = configuracao["Database:ConnectionString"] ?? string.Empty;

        switch (provedor)
        {
            case "Sqlite":
                servicos.AddMoneySqlite(stringConexao);
                break;
            default:
                throw new InvalidOperationException($"Provedor de banco não suportado: '{provedor}'.");
        }
    }
}
