using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class CrearTareaView : ContentPage
{
    public CrearTareaView(CrearTareaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}