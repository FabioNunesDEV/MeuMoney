using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Money.Data.Sqlite;
using Money.Desktop.ViewModels;
using Money.Services;
using Money.Services.Abstractions;

namespace Money.Desktop;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            _host = Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory)
                .ConfigureServices((context, services) =>
                {
                    services.AddMoneyServices();
                    AddDatabase(services, context.Configuration);

                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            await _host.StartAsync();

            using (var scope = _host.Services.CreateScope())
            {
                await scope.ServiceProvider.GetRequiredService<IDatabaseInitializer>().InitializeAsync();
            }

            _host.Services.GetRequiredService<MainWindow>().Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Meu Money", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }

    // Único ponto que escolhe o provedor de banco. Para usar SQL Server, referenciar
    // Money.Data.SqlServer e adicionar o caso correspondente aqui.
    private static void AddDatabase(IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"];
        var connectionString = configuration["Database:ConnectionString"] ?? string.Empty;

        switch (provider)
        {
            case "Sqlite":
                services.AddMoneySqlite(connectionString);
                break;
            default:
                throw new InvalidOperationException($"Provedor de banco não suportado: '{provider}'.");
        }
    }
}
