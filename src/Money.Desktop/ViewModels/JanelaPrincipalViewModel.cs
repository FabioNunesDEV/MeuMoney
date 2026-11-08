using CommunityToolkit.Mvvm.ComponentModel;

namespace Money.Desktop.ViewModels;

/// <summary>Estado da janela principal.</summary>
public partial class JanelaPrincipalViewModel : ObservableObject
{
    [ObservableProperty]
    private string _titulo = "Meu Money";
}
