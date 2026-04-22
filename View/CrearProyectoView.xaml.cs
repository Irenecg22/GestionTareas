using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class CrearProyectoView : ContentPage
{
	public CrearProyectoView(CrearProyectoViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}