using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using GestionTareas.View; // Asegúrate de importar las vistas
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

// Recibe el ProyectoId desde la lista de proyectos
[QueryProperty(nameof(ProyectoId), "ProyectoId")]
public partial class TareaViewModel : ObservableObject
{
    private readonly IRestService<Tarea> _tareaService;

    // Esta es la propiedad que le faltaba a tu código
    [ObservableProperty]
    private int proyectoId;

    [ObservableProperty]
    private ObservableCollection<Tarea> tareas;

    [ObservableProperty]
    private Tarea tareaSeleccionada;

    public IRelayCommand<Tarea> VerDetallesCommand { get; }
    public IAsyncRelayCommand LoadDataCommand { get; }

    public TareaViewModel(IRestService<Tarea> tareaService)
    {
        _tareaService = tareaService;
        VerDetallesCommand = new RelayCommand<Tarea>(VerDetalles);
        LoadDataCommand = new AsyncRelayCommand(LoadData);

        // Inicializamos la colección
        Tareas = new ObservableCollection<Tarea>();
    }

    private async Task LoadData()
    {
        try
        {
            // Aquí podrías filtrar por ProyectoId si tu API lo permite
            var lista = await _tareaService.GetAllAsync();

            if (lista != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Tareas = new ObservableCollection<Tarea>(lista);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void VerDetalles(Tarea tarea)
    {
        if (tarea == null) return;

        await Shell.Current.GoToAsync(
            "tareaDetalle",
            new ShellNavigationQueryParameters
            {
                { "Tarea", tarea }
            });
    }
}