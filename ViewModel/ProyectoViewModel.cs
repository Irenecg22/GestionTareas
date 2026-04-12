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

    public ProyectoViewModel(IRestService<Proyecto> proyectoService)
    {
        _proyectoService = proyectoService;
        VerDetallesCommand = new RelayCommand<Proyecto>(VerDetalles);
        LoadDataCommand = new AsyncRelayCommand(LoadData);
        Task.Run(async () => await LoadData());
    }

    private async Task LoadData()
    {
        var lista = await _proyectoService.GetAllAsync();
        Proyectos = new ObservableCollection<Proyecto>(lista);
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
