using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

public partial class ProyectoViewModel : ObservableObject
{
    private readonly IRestService<Proyecto> _proyectoService;

    [ObservableProperty]
    private ObservableCollection<Proyecto> proyectos;

    [ObservableProperty]
    private Proyecto proyectoSeleccionado;

    public IRelayCommand<Proyecto> VerDetallesCommand { get; }
    public IAsyncRelayCommand LoadDataCommand { get; }
    public IAsyncRelayCommand<Proyecto> EliminarProyectoCommand { get; }

    public ProyectoViewModel(IRestService<Proyecto> proyectoService)
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

        bool eliminado = await _proyectoService.DeleteAsync(proyecto.Id);

        if (eliminado)
        {
            await Shell.Current.DisplayAlert("Éxito", "Proyecto eliminado correctamente.", "OK");
            await RecargarProyectosAsync();
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