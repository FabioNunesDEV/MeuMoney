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
using Microsoft.Extensions.Logging;
using Money.Api;
using Money.Data.Sqlite;
using Money.Services;
using Money.Services.Abstracoes;
using Serilog;
using Serilog.Extensions.Logging;

namespace Money.Desktop;

/// <summary>
/// Composition root: sobe o servidor local (API e telas), prepara o banco e abre a janela.
/// </summary>
public partial class App : Application
{
    private WebApplication? servidor;
    private Serilog.Core.Logger? arquivoLog;
    private Microsoft.Extensions.Logging.ILogger? log;

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

            var pastaDados = Environment.ExpandEnvironmentVariables(
                builder.Configuration["PastaDados"]
                ?? throw new InvalidOperationException("A configuração 'PastaDados' não foi informada."));
            var log = this.LigarRegistroFalhas(builder, pastaDados);

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

            this.MainWindow = new JanelaPrincipal(new Uri(endereco), pastaDados, log);
            this.MainWindow.Show();
        }
        catch (Exception ex)
        {
            this.log?.LogCritical(ex, "Falha ao iniciar o aplicativo.");
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

        this.arquivoLog?.Dispose();
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

    /// <summary>
    /// Liga o registro de falhas: só erros, num arquivo por dia em <c>logs</c>, guardando 30 dias. Vale
    /// para o host, o EF Core e o próprio aplicativo, inclusive as exceções não tratadas.
    /// </summary>
    /// <param name="builder">Construtor do servidor local.</param>
    /// <param name="pastaDados">Pasta de dados do aplicativo.</param>
    /// <returns>O registro de falhas do próprio aplicativo.</returns>
    private Microsoft.Extensions.Logging.ILogger LigarRegistroFalhas(WebApplicationBuilder builder, string pastaDados)
    {
        // Sem buffer: cada falha chega ao disco na hora, mesmo que o processo caia em seguida.
        this.arquivoLog = new LoggerConfiguration()
            .MinimumLevel.Error()
            .WriteTo.File(
                Path.Combine(pastaDados, "logs", "meumoney-.log"),
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true)
            .CreateLogger();

        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(this.arquivoLog);

        var log = new SerilogLoggerFactory(this.arquivoLog).CreateLogger<App>();
        this.log = log;

        // Uma exceção não tratada na janela também chega aqui, pois encerra o processo.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            log.LogCritical(e.ExceptionObject as Exception, "Falha não tratada; o aplicativo será encerrado.");
        TaskScheduler.UnobservedTaskException += (_, e) =>
            log.LogError(e.Exception, "Falha não tratada numa tarefa em segundo plano.");
        return log;
    }
}
