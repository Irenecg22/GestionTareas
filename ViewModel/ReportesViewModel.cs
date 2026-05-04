using CommunityToolkit.Mvvm.ComponentModel;
using Microcharts;
using SkiaSharp;
using GestionTareas.Services;
using GestionTareas.Model;
using System.Diagnostics;

namespace GestionTareas.ViewModel;

public partial class ReportesViewModel : ObservableObject
{
    private readonly ProyectoService _proyectoService;
    private readonly TareaService _tareaService;
    private readonly UserService _userService;

    [ObservableProperty] private Microcharts.Chart _chartProyectos;
    [ObservableProperty] private Microcharts.Chart _chartTareasPorProyecto;
    [ObservableProperty] private Microcharts.Chart _chartTareasEstado;

    [ObservableProperty] private string _kpiUsuarios = "0";
    [ObservableProperty] private string _kpiNuevos7Dias = "0";
    [ObservableProperty] private string _kpiUsuariosActivos = "0";
    [ObservableProperty] private string _kpiTareas = "0";

    public ReportesViewModel(ProyectoService ps, TareaService ts, UserService us)
    {
        _proyectoService = ps;
        _tareaService = ts;
        _userService = us;
        _ = InicializarDashboard();
    }

    private async Task InicializarDashboard()
    {
        try
        {
            var proyectos = await _proyectoService.GetAllAsync() ?? new();
            var tareas = await _tareaService.GetAllAsync() ?? new();
            var usuarios = await _userService.GetAllAsync() ?? new();

            
            KpiUsuarios = proyectos.Count.ToString();
            KpiUsuariosActivos = usuarios.Count.ToString();
            KpiTareas = tareas.Count.ToString();

            var fechaLimite = DateTime.Now.AddDays(-7);
            KpiNuevos7Dias = proyectos.Count(p =>
            {
                if (DateTime.TryParse(p.FechaCreacion, out DateTime dt))
                    return dt >= fechaLimite;
                return false;
            }).ToString();

            bool isDark = App.Current.RequestedTheme == AppTheme.Dark;
            var textColor = isDark ? SKColors.White : SKColor.Parse("#2D3436");

            string[] coloresHex = { "#4834D4", "#6B21A8", "#A29BFE", "#00CEC9", "#FAB1A0", "#FD79A8" };

            var entriesUsuarios = usuarios.Select((u, index) => {
                var cantTareas = tareas.Count(t => t.UsuarioAsignadoId == u.Id);

                return new ChartEntry(cantTareas)
                {
                    Label = u.Nombre, 
                    ValueLabel = cantTareas.ToString(),
                    Color = SKColor.Parse(coloresHex[index % coloresHex.Length])
                };
            }).Where(e => e.Value > 0).ToArray(); 

            ChartProyectos = new DonutChart
            {
                Entries = entriesUsuarios,
                LabelTextSize = 24,
                LabelColor = textColor,
                BackgroundColor = SKColors.Transparent,
                HoleRadius = 0.5f
            };


            var entriesTareasProy = proyectos.Select(p => {
                var count = tareas.Count(t => t.ProyectoId == p.Id);
                return new ChartEntry(count)
                {
                    Label = p.Nombre.Length > 8 ? p.Nombre.Substring(0, 8) + ".." : p.Nombre,
                    ValueLabel = count.ToString(),
                    Color = SKColor.Parse("#6B21A8")
                };
            }).ToArray();

            ChartTareasPorProyecto = new BarChart
            {
                Entries = entriesTareasProy,
                LabelTextSize = 24,
                LabelColor = textColor,
                BackgroundColor = SKColors.Transparent
            };

            int done = tareas.Count(t => t.Estado == "Completada" || t.Estado == "Done");
            int doing = tareas.Count(t => t.Estado == "Progreso" || t.Estado == "Doing");
            int todo = tareas.Count(t => t.Estado == "Pendiente" || t.Estado == "To Do");

            ChartTareasEstado = new BarChart
            {
                Entries = new[] {
                new ChartEntry(todo) { Label = "To Do", ValueLabel = todo.ToString(), Color = SKColor.Parse("#FF7675") },
                new ChartEntry(doing) { Label = "Doing", ValueLabel = doing.ToString(), Color = SKColor.Parse("#6B21A8") },
                new ChartEntry(done) { Label = "Done", ValueLabel = done.ToString(), Color = SKColor.Parse("#55E6C1") }
            },
                LabelTextSize = 24,
                LabelColor = textColor,
                BackgroundColor = SKColors.Transparent
            };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" Error en Dashboard: {ex.Message}");
        }
    }
}