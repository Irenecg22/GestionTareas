using GestionTareas.ViewModel;
namespace GestionTareas.View;

public partial class TareaView : ContentPage
{
    public TareaView(TareaViewModel tareaViewModel)
    {
        BindingContext = tareaViewModel;
        InitializeComponent();
    }
}