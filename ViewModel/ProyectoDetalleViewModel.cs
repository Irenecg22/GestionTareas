using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

[QueryProperty(nameof(Proyecto), "Proyecto")]
public partial class ProyectoDetalleViewModel : ObservableObject
{
    private readonly TareaService _tareaService;

    [ObservableProperty]
    private Proyecto proyecto;

    [ObservableProperty]
    private ObservableCollection<Tarea> tareasFiltradas;

    public ProyectoDetalleViewModel()
    {
        _tareaService = new TareaService();
        TareasFiltradas = new ObservableCollection<Tarea>();
    }

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

        var tareasRelacionadas = todasLasTareas.Where(t => t.ProyectoId == proyectoId).ToList();

        TareasFiltradas.Clear();
        foreach (var tarea in tareasRelacionadas)
        {
            TareasFiltradas.Add(tarea);
        }
    }
}