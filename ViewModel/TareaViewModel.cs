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
    public List<Tarea> TareasEnProgreso => Tareas?.Where(t => t.Estado == "En Progreso" || t.Estado == "Doing" || t.Estado == "Progreso").ToList() ?? new();
    public List<Tarea> TareasCompletadas => Tareas?.Where(t => t.Estado == "Completada" || t.Estado == "Done").ToList() ?? new();

    [ObservableProperty]
    private Tarea tareaSeleccionada;

    public TareaViewModel(IRestService<Tarea> tareaService)
    {
        _tareaService = tareaService;
        Tareas = new ObservableCollection<Tarea>();

        _ = LoadData();
    }

    [RelayCommand]
    public async Task LoadData()
    {
        try
        {
            var lista = await _tareaService.GetAllAsync();

            if (lista != null)
            {
                var filtradas = ProyectoId > 0
                    ? lista.Where(t => t.ProyectoId == ProyectoId).ToList()
                    : lista;

                Tareas = new ObservableCollection<Tarea>(filtradas);
                RefreshColumns();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task VerDetalles(Tarea tarea)
    {
        if (tarea == null) return;
        await Shell.Current.GoToAsync("tareaDetalle", new Dictionary<string, object> { { "Tarea", tarea } });
    }

    [RelayCommand]
    private async Task EliminarTarea(Tarea tarea)
    {
        if (tarea == null) return;

        bool answer = await Shell.Current.DisplayAlert("Confirmar", $"¿Estás seguro de eliminar la tarea: {tarea.Titulo}?", "Sí", "No");

        if (answer)
        {
            var success = await _tareaService.DeleteAsync(tarea.Id);
            if (success)
            {
                Tareas.Remove(tarea);
                RefreshColumns();
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "No se pudo eliminar la tarea de la base de datos", "OK");
            }
        }
    }

    [RelayCommand]
    private async Task EditarTarea(Tarea tarea)
    {
        if (tarea == null) return;

        await Shell.Current.GoToAsync("CrearTareaView", new Dictionary<string, object>
        {
            { "TareaParaEditar", tarea }
        });
    }

    private void RefreshColumns()
    {
        OnPropertyChanged(nameof(TareasPendientes));
        OnPropertyChanged(nameof(TareasEnProgreso));
        OnPropertyChanged(nameof(TareasCompletadas));
    }
}