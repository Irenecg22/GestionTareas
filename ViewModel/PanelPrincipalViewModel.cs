using CommunityToolkit.Mvvm.ComponentModel;

namespace GestionTareas.ViewModel;

public partial class PanelPrincipalViewModel : ObservableObject
{
    [ObservableProperty]
    private string nombreUsuario = "Irene";

    [ObservableProperty]
    private int totalProyectos = 2;

    [ObservableProperty]
    private int totalTareas = 10;

    [ObservableProperty]
    private int tareasPendientes = 4;
}
