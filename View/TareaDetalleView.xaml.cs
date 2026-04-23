using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class TareaDetalleView : ContentPage
{
    public TareaDetalleView(TareaDetalleViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    
}
