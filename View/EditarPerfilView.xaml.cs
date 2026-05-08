using GestionTareas.ViewModel;
namespace GestionTareas.View;
public partial class EditarPerfilView : ContentPage
{
    public EditarPerfilView(EditarPerfilViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
