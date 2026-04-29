using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public class CrearTareaViewModel : INotifyPropertyChanged
{
    private readonly TareaService _tareaService;

    private string _titulo;
    public string Titulo { get => _titulo; set { _titulo = value; OnPropertyChanged(); } }

    private string _descripcion;
    public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(); } }

    private string _estado = "Pendiente";
    public string Estado { get => _estado; set { _estado = value; OnPropertyChanged(); } }

    private int _proyectoId;
    public int ProyectoId { get => _proyectoId; set { _proyectoId = value; OnPropertyChanged(); } }

    public ICommand CrearTareaCommand { get; }

    public CrearTareaViewModel(TareaService tareaService)
    {
        _tareaService = tareaService;
        CrearTareaCommand = new Command(async () => await CrearTareaAsync());
    }

    private async Task CrearTareaAsync()
    {
        if (string.IsNullOrWhiteSpace(Titulo) || ProyectoId <= 0)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Faltan campos obligatorios", "OK");
            return;
        }

        var nuevaTarea = new Tarea
        {
            Titulo = Titulo,
            Descripcion = Descripcion,
            Estado = Estado,
            ProyectoId = ProyectoId
        };

        if (await _tareaService.CreateAsync(nuevaTarea))
        {
            await Application.Current.MainPage.DisplayAlert("Éxito", "Tarea creada", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}