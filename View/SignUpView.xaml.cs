using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class SignUpView : ContentPage
{
    public SignUpView(SignUpViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}