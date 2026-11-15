// <copyright file="JanelaPrincipal.xaml.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Money.Desktop;

/// <summary>Janela do aplicativo: hospeda as telas web servidas pelo servidor local.</summary>
public partial class JanelaPrincipal : Window
{
    private readonly Uri endereco;

    /// <summary>Inicia uma nova instância da classe <see cref="JanelaPrincipal"/>.</summary>
    /// <param name="endereco">Endereço do servidor local que serve as telas.</param>
    public JanelaPrincipal(Uri endereco)
    {
        this.endereco = endereco;
        this.InitializeComponent();
        this.Loaded += this.AoCarregar;
    }

    private async void AoCarregar(object sender, RoutedEventArgs e)
    {
        try
        {
            // Os dados do navegador embutido ficam junto dos dados do aplicativo, não ao lado do executável.
            var pastaDados = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MeuMoney", "WebView2");
            var ambiente = await CoreWebView2Environment.CreateAsync(userDataFolder: pastaDados);
            await this.Navegador.EnsureCoreWebView2Async(ambiente);

            // É um aplicativo, não um navegador: sem menu, atalhos e barra de status do navegador.
            var ajustes = this.Navegador.CoreWebView2.Settings;
            ajustes.AreDefaultContextMenusEnabled = false;
            ajustes.AreBrowserAcceleratorKeysEnabled = false;
            ajustes.IsStatusBarEnabled = false;
            ajustes.IsZoomControlEnabled = false;

            this.Navegador.Source = this.endereco;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Meu Money", MessageBoxButton.OK, MessageBoxImage.Error);
            this.Close();
        }
    }
}
