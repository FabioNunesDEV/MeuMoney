// <copyright file="JanelaPrincipal.xaml.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using Money.Desktop.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace Money.Desktop;

/// <summary>Janela principal do aplicativo. Acompanha o tema claro/escuro do Windows.</summary>
public partial class JanelaPrincipal : FluentWindow
{
    /// <summary>
    /// Inicia uma nova instância da classe <see cref="JanelaPrincipal"/>, ligada ao seu ViewModel.
    /// </summary>
    /// <param name="viewModel">Estado exibido pela janela.</param>
    public JanelaPrincipal(JanelaPrincipalViewModel viewModel)
    {
        this.DataContext = viewModel;
        this.InitializeComponent();
        SystemThemeWatcher.Watch(this);
    }
}
