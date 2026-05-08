using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;
namespace GestionTareas.ViewModel;
[QueryProperty(nameof(ProyectoId), "ProyectoId")]
public partial class TareaViewModel : ObservableObject
{
    private readonly TareaService _tareaService;
    [ObservableProperty]
    private int proyectoId;
    [ObservableProperty]
    private ObservableCollection<Tarea> tareas;
    public List<Tarea> TareasPendientes => Tareas?.Where(t => t.Estado == "Pendiente").ToList() ?? new();
    public List<Tarea> TareasEnProgreso => Tareas?.Where(t => t.Estado == "En Progreso" || t.Estado == "Doing" || t.Estado == "Progreso").ToList() ?? new();
    public List<Tarea> TareasCompletadas => Tareas?.Where(t => t.Estado == "Completada" || t.Estado == "Done").ToList() ?? new();
    [ObservableProperty]
    private Tarea tareaSeleccionada;
    public TareaViewModel(TareaService tareaService)
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
            var (success, statusCode) = await _tareaService.DeleteWithStatusAsync(tarea.Id);
            if (success)
            {
                Tareas.Remove(tarea);
                RefreshColumns();
            }
            else if (statusCode == 403)
            {
                await Shell.Current.DisplayAlert("Sin permisos",
                    "No tienes permisos para eliminar tareas en este proyecto.", "OK");
            }
            else if (statusCode == 401)
            {
                await Shell.Current.DisplayAlert("Sesión expirada",
                    "Tu sesión ha expirado. Por favor, vuelve a iniciar sesión.", "OK");
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
        await Shell.Current.GoToAsync("EditarTareaView", new Dictionary<string, object>
        {
            { "TareaId", tarea.Id }
        });
    }
    private void RefreshColumns()
    {
        OnPropertyChanged(nameof(TareasPendientes));
        OnPropertyChanged(nameof(TareasEnProgreso));
        OnPropertyChanged(nameof(TareasCompletadas));
    }
}