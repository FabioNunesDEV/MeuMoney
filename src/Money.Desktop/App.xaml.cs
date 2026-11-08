using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Money.Data.Sqlite;
using Money.Desktop.ViewModels;
using Money.Services;
using Money.Services.Abstracoes;

namespace Money.Desktop;

/// <summary>Composition root: monta o host, prepara o banco e abre a janela principal.</summary>
public partial class App : Application
{
    private IHost? _host;

    /// <inheritdoc />
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureServices((contexto, servicos) =>
                {
                    servicos.AddMoneyServices();
                    AdicionarBanco(servicos, contexto.Configuration);

                    servicos.AddSingleton<JanelaPrincipalViewModel>();
                    servicos.AddSingleton<JanelaPrincipal>();
                })
                .Build();

            await _host.StartAsync();

            using (var escopo = _host.Services.CreateScope())
            {
                await escopo.ServiceProvider.GetRequiredService<IInicializadorBanco>().InicializarAsync();
            }

            _host.Services.GetRequiredService<JanelaPrincipal>().Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Meu Money", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    /// <inheritdoc />
    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
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
