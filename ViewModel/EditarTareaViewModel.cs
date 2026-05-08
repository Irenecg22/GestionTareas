using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Collections.ObjectModel;
using GestionTareas.Model;
using GestionTareas.Services;
namespace GestionTareas.ViewModel;
[QueryProperty(nameof(TareaId), "TareaId")]
public class EditarTareaViewModel : INotifyPropertyChanged
{
    private readonly TareaService _tareaService;
    private readonly ProyectoService _proyectoService;
    private int _tareaId;
    public int TareaId
    {
        get => _tareaId;
        set
        {
            _tareaId = value;
            OnPropertyChanged();
            if (value > 0)
            {
                _ = CargarTareaAsync(value);
            }
        }
    }
    private Tarea _tareaActual;
    public Tarea TareaActual
    {
        get => _tareaActual;
        set { _tareaActual = value; OnPropertyChanged(); }
    }
    private string _titulo;
    public string Titulo
    {
        get => _titulo;
        set { _titulo = value; OnPropertyChanged(); }
    }
    private string _descripcion;
    public string Descripcion
    {
        get => _descripcion;
        set { _descripcion = value; OnPropertyChanged(); }
    }
    private string _estado;
    public string Estado
    {
        get => _estado;
        set { _estado = value; OnPropertyChanged(); }
    }
    private string _nombreProyecto;
    public string NombreProyecto
    {
        get => _nombreProyecto;
        set { _nombreProyecto = value; OnPropertyChanged(); }
    }
    private ObservableCollection<string> _estadosDisponibles = new() { "Pendiente", "Progreso", "Completada" };
    public ObservableCollection<string> EstadosDisponibles
    {
        get => _estadosDisponibles;
        set { _estadosDisponibles = value; OnPropertyChanged(); }
    }
    private ObservableCollection<MiembroAsignacion> _miembrosDisponibles = new();
    public ObservableCollection<MiembroAsignacion> MiembrosDisponibles
    {
        get => _miembrosDisponibles;
        set { _miembrosDisponibles = value; OnPropertyChanged(); }
    }
    private MiembroAsignacion _miembroSeleccionado;
    public MiembroAsignacion MiembroSeleccionado
    {
        get => _miembroSeleccionado;
        set { _miembroSeleccionado = value; OnPropertyChanged(); }
    }
    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }
    public ICommand GuardarCambiosCommand { get; }
    public ICommand CancelarCommand { get; }
    public EditarTareaViewModel(TareaService tareaService, ProyectoService proyectoService)
    {
        _tareaService = tareaService;
        _proyectoService = proyectoService;
        GuardarCambiosCommand = new Command(async () => await GuardarCambiosAsync());
        CancelarCommand = new Command(async () => await CancelarAsync());
    }
    private async Task CargarTareaAsync(int id)
    {
        try
        {
            IsLoading = true;
            var tareas = await _tareaService.GetAllAsync();
            TareaActual = tareas.FirstOrDefault(t => t.Id == id);
            if (TareaActual != null)
            {
                Titulo = TareaActual.Titulo;
                Descripcion = TareaActual.Descripcion;
                Estado = TareaActual.Estado;
                NombreProyecto = TareaActual.NombreProyecto ?? $"Proyecto {TareaActual.ProyectoId}";
                await CargarMiembrosProyectoAsync(TareaActual.ProyectoId);
                if (TareaActual.UsuarioAsignadoId.HasValue)
                {
                    MiembroSeleccionado = MiembrosDisponibles.FirstOrDefault(m => m.IdUsuario == TareaActual.UsuarioAsignadoId);
                }
                else
                {
                    MiembroSeleccionado = MiembrosDisponibles.FirstOrDefault(m => m.EsSinAsignar);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error cargando tarea: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo cargar la tarea.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task CargarMiembrosProyectoAsync(int proyectoId)
    {
        try
        {
            var miembros = await _proyectoService.GetMiembrosAsync(proyectoId);
            MiembrosDisponibles.Clear();
            MiembrosDisponibles.Add(MiembroAsignacion.SinAsignar());
            foreach (var miembro in miembros)
            {
                MiembrosDisponibles.Add(MiembroAsignacion.FromProyectoUsuario(miembro));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error cargando miembros: {ex.Message}");
        }
    }
    private async Task GuardarCambiosAsync()
    {
        if (string.IsNullOrWhiteSpace(Titulo))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El título es obligatorio.", "OK");
            return;
        }
        if (TareaActual == null) return;
        try
        {
            IsLoading = true;
            var tareaActualizada = new Tarea
            {
                Id = TareaActual.Id,
                Titulo = Titulo,
                Descripcion = Descripcion,
                Estado = Estado,
                ProyectoId = TareaActual.ProyectoId,
                UsuarioAsignadoId = MiembroSeleccionado?.IdUsuario
            };
            var (success, statusCode) = await _tareaService.UpdateAsync(TareaActual.Id, tareaActualizada);
            if (success)
            {
                await Application.Current.MainPage.DisplayAlert("Éxito", "Tarea actualizada correctamente", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else if (statusCode == 403)
            {
                await Application.Current.MainPage.DisplayAlert("Sin permisos",
                    "No tienes permisos para editar esta tarea.", "OK");
            }
            else if (statusCode == 401)
            {
                await Application.Current.MainPage.DisplayAlert("Sesión expirada",
                    "Tu sesión ha expirado. Vuelve a iniciar sesión.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo actualizar la tarea.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error guardando cambios: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error", "Ocurrió un error al guardar.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    private async Task CancelarAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
