using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

[QueryProperty(nameof(TareaEditar), "TareaParaEditar")]
public class CrearTareaViewModel : INotifyPropertyChanged
{
    private readonly TareaService _tareaService;
    private readonly ProyectoService _proyectoService;

    private Tarea _tareaEditar;
    public Tarea TareaEditar
    {
        get => _tareaEditar;
        set
        {
            _tareaEditar = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(EsEdicion));
            OnPropertyChanged(nameof(TituloPantalla));
            OnPropertyChanged(nameof(TextoBoton));
            CargarTareaParaEditar(value);
        }
    }

    public bool EsEdicion => TareaEditar != null;
    public string TituloPantalla => EsEdicion ? "Editar Tarea" : "Nueva Tarea";
    public string TextoBoton => EsEdicion ? "Guardar Cambios" : "Guardar Tarea";

    private string _titulo;
    public string Titulo { get => _titulo; set { _titulo = value; OnPropertyChanged(); } }

    private string _descripcion;
    public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

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
        set { _mensajeError = value; OnPropertyChanged(); OnPropertyChanged(nameof(TieneMensaje)); }
    }

    public bool TieneMensaje => !string.IsNullOrEmpty(MensajeError);

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

    private void CargarTareaParaEditar(Tarea tarea)
    {
        if (tarea != null)
        {
            Titulo = tarea.Titulo;
            Descripcion = tarea.Descripcion;
            Estado = tarea.Estado;

            // Seleccionar el proyecto correspondiente en el picker
            ProyectoSeleccionado = ProyectosDisponibles.FirstOrDefault(p => p.Id == tarea.ProyectoId);
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
            Estado = Estado,
            ProyectoId = ProyectoSeleccionado.Id
        };

        bool success;
        int statusCode;

        if (EsEdicion)
        {
            // Actualizar tarea existente
            (success, statusCode) = await _tareaService.UpdateAsync(TareaEditar.Id, tarea);
        }
        else
        {
            // Crear nueva tarea
            (success, statusCode) = await _tareaService.CreateWithStatusAsync(tarea);
        }

        if (success)
        {
            string mensaje = EsEdicion ? "Tarea actualizada" : "Tarea creada";
            await Application.Current.MainPage.DisplayAlert("Éxito", mensaje, "OK");
            await Shell.Current.GoToAsync("..");
        }
        else if (statusCode == 403)
        {
            string mensaje = EsEdicion 
                ? "No tienes permisos para editar esta tarea." 
                : "No tienes permisos para crear tareas en este proyecto.";
            await Application.Current.MainPage.DisplayAlert("Sin permisos", mensaje, "OK");
        }
        else if (statusCode == 401)
        {
            await Application.Current.MainPage.DisplayAlert("Sesión expirada",
                "Tu sesión ha expirado. Vuelve a iniciar sesión.", "OK");
        }
        else
        {
            string mensaje = EsEdicion ? "No se pudo actualizar la tarea." : "No se pudo crear la tarea.";
            await Application.Current.MainPage.DisplayAlert("Error", mensaje, "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}