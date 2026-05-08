using GestionTareas.ViewModel;
namespace GestionTareas.View;
public partial class ProyectoView : ContentPage
{
    public ProyectoView(ProyectoViewModel proyectoViewModel)
    {
        InitializeComponent();
        BindingContext = proyectoViewModel;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ProyectoViewModel vm)
        {
            await vm.RecargarProyectosAsync();
        }
    }
    private async void OnNuevoProyectoClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(CrearProyectoView));
    }
}