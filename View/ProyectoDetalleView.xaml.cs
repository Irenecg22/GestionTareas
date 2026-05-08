using GestionTareas.ViewModel;
namespace GestionTareas.View;
public partial class ProyectoDetalleView : ContentPage
{
    public ProyectoDetalleView(ProyectoDetalleViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}