using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public partial class TareaViewModel : ObservableObject
{
    private readonly IRestService<Tarea> _tareaService;

    [ObservableProperty]
    private List<Tarea> tareas;

    [ObservableProperty]
    private Tarea _tareaSeleccionada;


    public TareaViewModel(IRestService<Tarea> tareaService)
    {
        _tareaService = tareaService;
        LoadData();
    }

    private async void LoadData() => Tareas = await _tareaService.GetAllAsync();

    [RelayCommand]
    private async void VerDetalles(Tarea tarea)
    {
        await Shell.Current.GoToAsync(
            "tareaDetalle",
            new ShellNavigationQueryParameters
            {
                { "Tarea", tarea }
            });
    }

}
