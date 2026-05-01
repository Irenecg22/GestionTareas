using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class ReportesView : ContentPage
{
	public ReportesView(ReportesViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}