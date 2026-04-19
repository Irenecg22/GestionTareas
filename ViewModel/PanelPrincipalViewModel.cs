using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

public partial class PanelPrincipalViewModel : ObservableObject
{
    private readonly IRestService<Proyecto> _proyectoService;
    private readonly IRestService<Usuario> _usuarioService;

    [ObservableProperty] private ObservableCollection<Proyecto> proyectos;
    [ObservableProperty] private Usuario usuarioLogueado;
    [ObservableProperty] private int totalProyectos;
    [ObservableProperty] private double porcentajeGlobal;
    [ObservableProperty] private double porcentajeEnProgreso;
    [ObservableProperty] private double porcentajePendiente;

    public PanelPrincipalViewModel(
        IRestService<Proyecto> proyectoService,
        IRestService<Usuario> usuarioService)
    {
        _proyectoService = proyectoService;
        _usuarioService = usuarioService;
        _ = LoadData();
    }

    private async Task LoadData()
    {
        var listaProyectos = await _proyectoService.GetAllAsync() ?? new List<Proyecto>();
        var listaUsuarios = await _usuarioService.GetAllAsync() ?? new List<Usuario>();

        
        UsuarioLogueado = listaUsuarios.FirstOrDefault();

        var todasLasTareas = listaProyectos.SelectMany(p => p.Tareas ?? new List<Tarea>()).ToList();
        int totalTareas = todasLasTareas.Count;

        if (totalTareas > 0)
        {
            PorcentajeGlobal = (double)todasLasTareas.Count(t => t.Estado == "Finalizada") / totalTareas;
            PorcentajeEnProgreso = (double)todasLasTareas.Count(t => t.Estado == "EnProgreso") / totalTareas;
            PorcentajePendiente = (double)todasLasTareas.Count(t => t.Estado == "Pendiente") / totalTareas;
        }

        Proyectos = new ObservableCollection<Proyecto>(listaProyectos);
        TotalProyectos = Proyectos.Count;
    }
}