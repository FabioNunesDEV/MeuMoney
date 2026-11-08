using CommunityToolkit.Mvvm.ComponentModel;

namespace Money.Desktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = "Meu Money";
}
