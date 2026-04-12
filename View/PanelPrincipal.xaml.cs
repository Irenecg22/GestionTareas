using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class PanelPrincipal : ContentPage
{
	public PanelPrincipal(PanelPrincipalViewModel panelPrincipalViewModel)
	{
		BindingContext = panelPrincipalViewModel;
		InitializeComponent();
	}
}