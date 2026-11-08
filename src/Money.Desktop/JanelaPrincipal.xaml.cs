using Money.Desktop.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Money.Desktop;

/// <summary>Janela principal do aplicativo. Acompanha o tema claro/escuro do Windows.</summary>
public partial class JanelaPrincipal : FluentWindow
{
    /// <summary>Cria a janela ligada ao seu ViewModel.</summary>
    /// <param name="viewModel">Estado exibido pela janela.</param>
    public JanelaPrincipal(JanelaPrincipalViewModel viewModel)
    {
        DataContext = viewModel;
        InitializeComponent();
        SystemThemeWatcher.Watch(this);
    }
}
