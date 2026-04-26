using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GestionTareas.ViewModel;

public partial class PanelPrincipalViewModel : ObservableObject
{
    private readonly IRestService<Proyecto> _proyectoService;
    private readonly IRestService<Usuario> _usuarioService;
    private readonly IRestService<Tarea> _tareaService; 

    [ObservableProperty] private ObservableCollection<Proyecto> proyectos;
    [ObservableProperty] private ObservableCollection<Tarea> todasLasTareas;
    [ObservableProperty] private Usuario usuarioLogueado;
    [ObservableProperty] private int totalProyectos;
    [ObservableProperty] private int totalTareas;
    [ObservableProperty] private int tareasPendientesCount;
    [ObservableProperty] private int tareasEnProgresoCount;
    [ObservableProperty] private int tareasFinalizadasCount;
    [ObservableProperty] private double porcentajeGlobal;
    [ObservableProperty] private string fechaActual;

    public PanelPrincipalViewModel(
        IRestService<Proyecto> proyectoService,
        IRestService<Usuario> usuarioService,
        IRestService<Tarea> tareaService) 
    {
        _proyectoService = proyectoService;
        _usuarioService = usuarioService;
        _tareaService = tareaService;

        Proyectos = new ObservableCollection<Proyecto>();
        TodasLasTareas = new ObservableCollection<Tarea>();

        _ = LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            var listaProyectos = await _proyectoService.GetAllAsync() ?? new List<Proyecto>();
            var listaUsuarios = await _usuarioService.GetAllAsync() ?? new List<Usuario>();
            var listaTareasTotal = await _tareaService.GetAllAsync() ?? new List<Tarea>(); 

            var user = listaUsuarios.FirstOrDefault();
            var fecha = DateTime.Now.ToString("dddd, dd 'de' MMMM");

            int totalT = listaTareasTotal.Count;
            int tFin = listaTareasTotal.Count(t => t.Estado == "Completada" || t.Estado == "Done");
            int tProg = listaTareasTotal.Count(t => t.Estado == "Progreso" || t.Estado == "Doing" || t.Estado == "En Progreso");
            int tPend = listaTareasTotal.Count(t => t.Estado == "Pendiente" || t.Estado == "To Do");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                UsuarioLogueado = user;
                FechaActual = fecha;
                TotalProyectos = listaProyectos.Count;
                TotalTareas = totalT;
                TareasPendientesCount = tPend;
                TareasEnProgresoCount = tProg;
                TareasFinalizadasCount = tFin;
                PorcentajeGlobal = totalT > 0 ? (double)tFin / totalT : 0;

                Proyectos = new ObservableCollection<Proyecto>(listaProyectos);
                TodasLasTareas = new ObservableCollection<Tarea>(listaTareasTotal);
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error: {ex.Message}");
        }
    }
}