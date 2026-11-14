// <copyright file="JanelaPrincipalViewModel.cs" company="Fábio Nunes">
// Copyright (c) 2026 Fábio Nunes. Licenciado sob a licença MIT.
// </copyright>

using CommunityToolkit.Mvvm.ComponentModel;

namespace Money.Desktop.ViewModels;

/// <summary>Estado da janela principal.</summary>
public partial class JanelaPrincipalViewModel : ObservableObject
{
    [ObservableProperty]
    private string titulo = "Meu Money";
}
