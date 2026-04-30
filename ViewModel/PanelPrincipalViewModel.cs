using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace GestionTareas.ViewModel;

public partial class PanelPrincipalViewModel : ObservableObject
{
    private readonly ProyectoService _proyectoService;
    private readonly UserService _userService;
    private readonly TareaService _tareaService;

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
        ProyectoService proyectoService,
        UserService userService,
        TareaService tareaService)
    {
        _proyectoService = proyectoService;
        _userService = userService;
        _tareaService = tareaService;

        Proyectos = new ObservableCollection<Proyecto>();
        TodasLasTareas = new ObservableCollection<Tarea>();

        _ = LoadData();
    }

    private async Task LoadData()
    {
        try
        {
            Debug.WriteLine("📊 PanelPrincipalViewModel: Cargando datos del dashboard...");

            var listaProyectos = await _proyectoService.GetAllAsync() ?? new List<Proyecto>();
            var listaTareasTotal = await _tareaService.GetAllAsync() ?? new List<Tarea>();

            Debug.WriteLine("👤 Obteniendo usuario autenticado actual...");
            var user = await _userService.GetCurrentUserAsync();

            if (user != null)
            {
                Debug.WriteLine($"✅ Usuario cargado en Dashboard: {user.Nombre} ({user.Email})");
            }
            else
            {
                Debug.WriteLine("⚠️ No se pudo obtener el usuario actual para el Dashboard");
            }

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
            Debug.WriteLine($"❌ Error en PanelPrincipalViewModel.LoadData: {ex.Message}");
        }
    }
}