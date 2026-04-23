using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

[QueryProperty(nameof(ProyectoId), "ProyectoId")]
public partial class TareaViewModel : ObservableObject
{
    private readonly IRestService<Tarea> _tareaService;

    [ObservableProperty]
    private int proyectoId;

    [ObservableProperty]
    private ObservableCollection<Tarea> tareas;

    public List<Tarea> TareasPendientes => Tareas?.Where(t => t.Estado == "Pendiente").ToList() ?? new();
    public List<Tarea> TareasEnProgreso => Tareas?.Where(t => t.Estado == "En Progreso" || t.Estado == "Doing").ToList() ?? new();
    public List<Tarea> TareasCompletadas => Tareas?.Where(t => t.Estado == "Completada" || t.Estado == "Done").ToList() ?? new();

    [ObservableProperty]
    private Tarea tareaSeleccionada;

    public IRelayCommand<Tarea> VerDetallesCommand { get; }
    public IAsyncRelayCommand LoadDataCommand { get; }

    public TareaViewModel(IRestService<Tarea> tareaService)
    {
        _tareaService = tareaService;
        VerDetallesCommand = new RelayCommand<Tarea>(VerDetalles);
        LoadDataCommand = new AsyncRelayCommand(LoadData);
        Tareas = new ObservableCollection<Tarea>();
    }

    private async Task LoadData()
    {
        try
        {
            var lista = await _tareaService.GetAllAsync();

            if (lista != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Tareas = new ObservableCollection<Tarea>(lista);
                    OnPropertyChanged(nameof(TareasPendientes));
                    OnPropertyChanged(nameof(TareasEnProgreso));
                    OnPropertyChanged(nameof(TareasCompletadas));
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
        await Shell.Current.GoToAsync("tareaDetalle", new Dictionary<string, object> { { "Tarea", tarea } });
    }
}