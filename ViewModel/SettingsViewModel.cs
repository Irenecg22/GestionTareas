using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IRestService<Usuario> _userService;

    [ObservableProperty]
    private Usuario usuarioActual;

    [ObservableProperty]
    private List<AppTheme> themes = new()
    {
        AppTheme.Unspecified,
        AppTheme.Light,
        AppTheme.Dark
    };

    [ObservableProperty]
    private AppTheme selectedTheme;

    public SettingsViewModel(IRestService<Usuario> usuarioService)
    {
        _userService = usuarioService;
        selectedTheme = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
        _ = LoadUserData();
    }

    [RelayCommand]
    private void SaveSettings(AppTheme theme)
    {
        SelectedTheme = theme;
    }

    private async Task LoadUserData()
    {
        try
        {
            var usuarios = await _userService.GetAllAsync();
            var usuario = usuarios?.FirstOrDefault();
            if (usuario != null)
            {
                MainThread.BeginInvokeOnMainThread(() => UsuarioActual = usuario);
            }
        }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine(ex.Message); }
    }

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        if (Application.Current != null)
            MainThread.BeginInvokeOnMainThread(() => Application.Current.UserAppTheme = value);
    }
}