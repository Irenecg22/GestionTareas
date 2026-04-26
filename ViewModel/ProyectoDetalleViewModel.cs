using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

[QueryProperty(nameof(Proyecto), "Proyecto")]
public partial class ProyectoDetalleViewModel : ObservableObject
{
    // Inyectamos el servicio de tareas
    private readonly TareaService _tareaService;

    [ObservableProperty]
    private Proyecto proyecto;

    // Usamos una propiedad para las tareas que el XAML pueda observar
    [ObservableProperty]
    private ObservableCollection<Tarea> tareasFiltradas;

    public ProyectoDetalleViewModel()
    {
        _tareaService = new TareaService();
        TareasFiltradas = new ObservableCollection<Tarea>();
    }

    // Este método se ejecuta automáticamente cuando el "Proyecto" llega por navegación
    partial void OnProyectoChanged(Proyecto value)
    {
        if (value != null)
        {
            _ = CargarTareasDelProyecto(value.Id);
        }
    }

    private async Task CargarTareasDelProyecto(int proyectoId)
    {
        var todasLasTareas = await _tareaService.GetAllAsync();

        // Filtramos las tareas que pertenecen a este proyecto
        var tareasRelacionadas = todasLasTareas.Where(t => t.ProyectoId == proyectoId).ToList();

        TareasFiltradas.Clear();
        foreach (var tarea in tareasRelacionadas)
        {
            TareasFiltradas.Add(tarea);
        }
    }
}