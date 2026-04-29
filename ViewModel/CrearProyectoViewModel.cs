using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public class CrearProyectoViewModel : INotifyPropertyChanged
{
    private readonly ProyectoService _proyectoService;

    private string nombre;
    public string Nombre
    {
        get => nombre;
        set
        {
            nombre = value;
            OnPropertyChanged();
        }
    }

    private string descripcion;
    public string Descripcion
    {
        get => descripcion;
        set
        {
            descripcion = value;
            OnPropertyChanged();
        }
    }

    private DateTime fechaCreacion = DateTime.Today;
    public DateTime FechaCreacion
    {
        get => fechaCreacion;
        set
        {
            fechaCreacion = value;
            OnPropertyChanged();
        }
    }

    public ICommand CrearProyectoCommand { get; }

    public CrearProyectoViewModel(ProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
        CrearProyectoCommand = new Command(async () => await CrearProyectoAsync());
    }

    private async Task CrearProyectoAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El nombre es obligatorio.", "OK");
            return;
        }

        Proyecto nuevoProyecto = new Proyecto
        {
            Nombre = Nombre,
            Descripcion = Descripcion,
            FechaCreacion = FechaCreacion.ToString("yyyy-MM-dd")
        };

        bool creado = await _proyectoService.CreateAsync(nuevoProyecto);

        if (creado)
        {
            await Application.Current.MainPage.DisplayAlert("Éxito", "Proyecto creado correctamente.", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else
        {
            await Application.Current.MainPage.DisplayAlert("Error", "No se pudo crear el proyecto.", "OK");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string nombrePropiedad = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nombrePropiedad));
    }
}