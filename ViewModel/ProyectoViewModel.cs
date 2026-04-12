using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public partial class ProyectoViewModel : ObservableObject
{
    private readonly IRestService<Proyecto> _proyectoService;
    private Proyecto _proyectoSeleccionado;

    [ObservableProperty]
    private List<Proyecto> proyectos;

    public IRelayCommand<Proyecto> VerDetallesCommand { get; }

    public ProyectoViewModel(IRestService<Proyecto> proyectoService)
    {
        _proyectoService = proyectoService;
        VerDetallesCommand = new RelayCommand<Proyecto>(VerDetalles);
        LoadData();
    }

    private async void LoadData()
    {
        Proyectos = await _proyectoService.GetAllAsync();
    }

    private async void VerDetalles(Proyecto proyecto)
    {
        await Shell.Current.GoToAsync(
            "proyectoDetalle",
            new ShellNavigationQueryParameters
            {
                { "Proyecto", proyecto }
            });
    }

    public Proyecto ProyectoSeleccionado
    {
        get => _proyectoSeleccionado;
        set
        {
            _proyectoSeleccionado = value;
            OnPropertyChanged();
        }
    }
}
