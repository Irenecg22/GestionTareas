using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class TareaView : ContentPage
{
    private readonly TareaViewModel _viewModel;

    public TareaView(TareaViewModel tareaViewModel)
    {
        InitializeComponent();
        _viewModel = tareaViewModel;
        BindingContext = _viewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is TareaViewModel vm)
        {
            await vm.LoadDataCommand.ExecuteAsync(null);
        }
    }

    private async void OnNuevaTareaClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrearTareaView));
    }
}