using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Collections.ObjectModel;

namespace GestionTareas.ViewModel;

public partial class TareaViewModel : ObservableObject
{
    private readonly IRestService<Tarea> _tareaService;

    [ObservableProperty]
    private ObservableCollection<Tarea> tareas;

    [ObservableProperty]
    private Tarea tareaSeleccionada;
    public IRelayCommand<Tarea> VerDetallesCommand { get; }
    public IAsyncRelayCommand LoadDataCommand { get; }

    public TareaViewModel(IRestService<Tarea> tareaService)
    {
        _tareaService = tareaService;
        VerDetallesCommand = new RelayCommand<Tarea>(VerDetalles);
        LoadDataCommand = new AsyncRelayCommand(LoadData);
        Task.Run(async () => await LoadData());
    }

    private async Task LoadData()
    {
        try
        {
            var lista = await _tareaService.GetAllAsync();

            if (lista != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Tareas = new ObservableCollection<Tarea>(lista);
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
        }
    }
    private async void VerDetalles(Tarea tarea)
    {
        if (tarea == null) return;

        await Shell.Current.GoToAsync(
            "tareaDetalle",
            new ShellNavigationQueryParameters
            {
                { "Tarea", tarea }
            });
    }
}