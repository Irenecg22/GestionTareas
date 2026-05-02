using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

/// <summary>
/// ViewModel para CREAR nuevas tareas (no editar)
/// </summary>
public class CrearTareaViewModel : INotifyPropertyChanged
{
    private readonly TareaService _tareaService;
    private readonly ProyectoService _proyectoService;

    private string _titulo;
    public string Titulo { get => _titulo; set { _titulo = value; OnPropertyChanged(); } }

    private string _descripcion;
    public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

    // Estado siempre es "Pendiente" por defecto
    private string _estado = "Pendiente";
    public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(); } }

    private ObservableCollection<Proyecto> _proyectosDisponibles = new();
    public ObservableCollection<Proyecto> ProyectosDisponibles
    {
        get => _proyectosDisponibles;
        set { _proyectosDisponibles = value; OnPropertyChanged(); }
    }

    private Proyecto _proyectoSeleccionado;
    public Proyecto ProyectoSeleccionado
    {
        get => _proyectoSeleccionado;
        set { _proyectoSeleccionado = value; OnPropertyChanged(); }
    }

    private string _mensajeError;
    public string MensajeError
    {
        get => _mensajeError;
        set { _mensajeError = value; OnPropertyChanged(); }
    }

    public ICommand CrearTareaCommand { get; }

    public CrearTareaViewModel(TareaService tareaService, ProyectoService proyectoService)
    {
        _tareaService = tareaService;
        _proyectoService = proyectoService;
        CrearTareaCommand = new Command(async () => await CrearTareaAsync());
        _ = CargarProyectosAsync();
    }

    private async Task CargarProyectosAsync()
    {
        try
        {
            var proyectos = await _proyectoService.GetAllAsync();
            ProyectosDisponibles = new ObservableCollection<Proyecto>(proyectos);

            if (proyectos.Count == 0)
            {
                MensajeError = "No tienes proyectos disponibles. Crea o únete a un proyecto primero.";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error cargando proyectos: {ex.Message}");
        }
    }

    private async Task CrearTareaAsync()
    {
        MensajeError = null;

        if (string.IsNullOrWhiteSpace(Titulo))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El título es obligatorio.", "OK");
            return;
        }

        if (ProyectoSeleccionado == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Selecciona un proyecto.", "OK");
            return;
        }

        var tarea = new Tarea
        {
            Titulo = Titulo,
            Descripcion = Descripcion,
            Estado = "Pendiente", // Siempre Pendiente al crear
            ProyectoId = ProyectoSeleccionado.Id,
            UsuarioAsignadoId = null // Sin asignar inicialmente
        };

        var (success, statusCode) = await _tareaService.CreateWithStatusAsync(tarea);

        if (success)
        {
            await Application.Current.MainPage.DisplayAlert("Éxito", "Tarea creada correctamente", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else if (statusCode == 403)
        {
            await Application.Current.MainPage.DisplayAlert("Sin permisos",
                "No tienes permisos para crear tareas en este proyecto.", "OK");
        }
        else if (statusCode == 401)
        {
            await Application.Current.MainPage.DisplayAlert("Sesión expirada",
                "Tu sesión ha expirado. Vuelve a iniciar sesión.", "OK");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo crear la tarea.", "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
