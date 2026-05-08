using GestionTareas.ViewModel;
namespace GestionTareas.View;
public partial class SettingsView : ContentPage
{
	private readonly SettingsViewModel _viewModel;
	public SettingsView(SettingsViewModel settingsViewModel)
	{
		InitializeComponent();
		_viewModel = settingsViewModel;
		BindingContext = _viewModel;
	}
	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await _viewModel.RefreshUserData();
	}
}
