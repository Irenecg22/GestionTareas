using GestionTareas.ViewModel;
namespace GestionTareas.View;
public partial class EditarTareaView : ContentPage
{
    public EditarTareaView(EditarTareaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
