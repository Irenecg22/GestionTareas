using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class ProyectoView : ContentPage
{
	public ProyectoView(ProyectoViewModel proyectoViewModel)
	{
        BindingContext = proyectoViewModel;
        InitializeComponent();
    }
}