using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

public partial class ProyectoViewModel : ObservableObject
{
    private readonly ProyectoService _proyectoService;

    [ObservableProperty]
    private ObservableCollection<Proyecto> proyectos;

    [ObservableProperty]
    private Proyecto proyectoSeleccionado;

    public IRelayCommand<Proyecto> VerDetallesCommand { get; }
    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand<Proyecto> EliminarProyectoCommand { get; }

    public ProyectoViewModel(ProyectoService proyectoService)
    {
        _proyectoService = proyectoService;
        VerDetallesCommand = new RelayCommand<Proyecto>(VerDetalles);
        LoadDataCommand = new AsyncRelayCommand(LoadDataAsync);
        EliminarProyectoCommand = new AsyncRelayCommand<Proyecto>(EliminarProyectoAsync);
    }

    public async Task RecargarProyectosAsync()
    {
        await LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        var lista = await _proyectoService.GetAllAsync();
        Proyectos = new ObservableCollection<Proyecto>(lista);
    }

    private async Task EliminarProyectoAsync(Proyecto proyecto)
    {
        if (proyecto == null) return;

        bool confirmar = await Shell.Current.DisplayAlert(
            "Confirmar eliminación",
            $"¿Seguro que quieres eliminar el proyecto \"{proyecto.Nombre}\"?",
            "Sí, eliminar",
            "Cancelar");

        if (!confirmar)
            return;

        var (eliminado, statusCode) = await _proyectoService.DeleteWithStatusAsync(proyecto.Id);

        if (eliminado)
        {
            await Shell.Current.DisplayAlert("Éxito", "Proyecto eliminado correctamente.", "OK");
            await RecargarProyectosAsync();
        }
        else if (statusCode == 403)
        {
            await Shell.Current.DisplayAlert("Sin permisos",
                "No tienes permisos para eliminar este proyecto.\nSolo el owner o un administrador puede hacerlo.", "OK");
        }
        else if (statusCode == 401)
        {
            await Shell.Current.DisplayAlert("Sesión expirada",
                "Tu sesión ha expirado. Por favor, vuelve a iniciar sesión.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar el proyecto.", "OK");
        }
    }

    private async void VerDetalles(Proyecto proyecto)
    {
        if (proyecto == null) return;

        await Shell.Current.GoToAsync(
            "proyectoDetalle",
            new ShellNavigationQueryParameters
            {
                { "Proyecto", proyecto }
            });
    }
}