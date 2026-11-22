// <copyright file="JanelaPrincipal.xaml.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.IO;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Logging;
using Microsoft.Web.WebView2.Core;

namespace Money.Desktop;

/// <summary>Janela do aplicativo: hospeda as telas web servidas pelo servidor local.</summary>
public partial class JanelaPrincipal : Window
{
    private readonly Uri endereco;
    private readonly string pastaDados;
    private readonly ILogger log;

    /// <summary>Inicia uma nova instância da classe <see cref="JanelaPrincipal"/>.</summary>
    /// <param name="endereco">Endereço do servidor local que serve as telas.</param>
    /// <param name="pastaDados">Pasta de dados do aplicativo; os dados do navegador embutido ficam nela.</param>
    /// <param name="log">Registro de falhas.</param>
    public JanelaPrincipal(Uri endereco, string pastaDados, ILogger log)
    {
        this.endereco = endereco;
        this.pastaDados = pastaDados;
        this.log = log;
        this.InitializeComponent();
        this.SourceInitialized += (_, _) => BarraTitulo.AtivarModoEscuro(this);
        this.Loaded += this.AoCarregar;
    }

    private async void AoCarregar(object sender, RoutedEventArgs e)
    {
        try
        {
            var ambiente = await CoreWebView2Environment.CreateAsync(
                userDataFolder: Path.Combine(this.pastaDados, "WebView2"));
            await this.Navegador.EnsureCoreWebView2Async(ambiente);

            // É um aplicativo, não um navegador: sem menu, atalhos e barra de status do navegador.
            var ajustes = this.Navegador.CoreWebView2.Settings;
            ajustes.AreDefaultContextMenusEnabled = false;
            ajustes.AreBrowserAcceleratorKeysEnabled = false;
            ajustes.IsStatusBarEnabled = false;
            ajustes.IsZoomControlEnabled = false;

            this.Navegador.CoreWebView2.WebMessageReceived += this.AoReceberMensagem;
            this.Navegador.Source = this.endereco;
        }
        catch (Exception ex)
        {
            this.log.LogCritical(ex, "Falha ao abrir o navegador embutido.");
            MessageBox.Show(ex.Message, "Meu Money", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }
    }

    /// <summary>
    /// Trata as mensagens das telas. Hoje só o tema: <c>{ tipo: 'tema', corBarra, corTexto }</c>, que
    /// pinta a barra de título. Mensagem desconhecida ou malformada é ignorada.
    /// </summary>
    /// <param name="sender">Origem do evento.</param>
    /// <param name="e">Mensagem recebida.</param>
    private void AoReceberMensagem(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            using var mensagem = JsonDocument.Parse(e.WebMessageAsJson);
            var raiz = mensagem.RootElement;
            if (raiz.ValueKind == JsonValueKind.Object
                && raiz.TryGetProperty("tipo", out var tipo) && tipo.ValueKind == JsonValueKind.String
                && tipo.GetString() == "tema"
                && raiz.TryGetProperty("corBarra", out var corBarra) && corBarra.ValueKind == JsonValueKind.String
                && raiz.TryGetProperty("corTexto", out var corTexto) && corTexto.ValueKind == JsonValueKind.String)
            {
                BarraTitulo.Pintar(this, corBarra.GetString(), corTexto.GetString());
            }
        }
        catch (JsonException)
        {
            // A mensagem vem das nossas telas; uma malformada não deve derrubar a janela.
        }
    }
}
