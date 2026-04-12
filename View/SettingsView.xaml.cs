using GestionTareas.ViewModel;

namespace GestionTareas.View;

public partial class SettingsView : ContentPage
{
	public SettingsView(SettingsViewModel settingsViewModel )
	{
        BindingContext = settingsViewModel;
        InitializeComponent();
	}
}