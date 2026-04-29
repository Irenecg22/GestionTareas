using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;

namespace GestionTareas.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly UserService _userService;

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

    public SettingsViewModel(UserService userService)
    {
        _userService = userService;
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
            System.Diagnostics.Debug.WriteLine("🔍 SettingsViewModel: Cargando datos del usuario actual...");

            var usuario = await _userService.GetCurrentUserAsync();

            if (usuario != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Usuario cargado: {usuario.Nombre} ({usuario.Email})");
                MainThread.BeginInvokeOnMainThread(() => UsuarioActual = usuario);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No se pudo obtener el usuario actual");
            }
        }
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine($"❌ Error en LoadUserData: {ex.Message}"); 
        }
    }

    partial void OnSelectedThemeChanged(AppTheme value)
    {
        if (Application.Current != null)
            MainThread.BeginInvokeOnMainThread(() => Application.Current.UserAppTheme = value);
    }
}