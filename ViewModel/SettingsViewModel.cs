using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GestionTareas.ViewModel;

public partial class SettingsViewModel : ObservableObject
{
    private readonly UserService _userService;

    [ObservableProperty]
    private Usuario usuarioActual;

    [ObservableProperty]
    private string inicialesUsuario;

    [ObservableProperty]
    private string nombreUsuario;

    [ObservableProperty]
    private string emailUsuario;

    [ObservableProperty]
    private string idUsuario;

    [ObservableProperty]
    private string rolGlobal;

    [ObservableProperty]
    private List<string> temasDisplay = new() { "Sistema", "Claro", "Oscuro" };

    [ObservableProperty]
    private string temaSeleccionado;

    private readonly Dictionary<string, AppTheme> themesMap = new()
    {
        { "Sistema", AppTheme.Unspecified },
        { "Claro", AppTheme.Light },
        { "Oscuro", AppTheme.Dark }
    };

    public SettingsViewModel(UserService userService)
    {
        _userService = userService;

        var currentTheme = Application.Current?.UserAppTheme ?? AppTheme.Unspecified;
        temaSeleccionado = themesMap.FirstOrDefault(x => x.Value == currentTheme).Key ?? "Sistema";

        _ = LoadUserData();
    }

    [RelayCommand]
    private void SaveSettings(string tema)
    {
        if (themesMap.TryGetValue(tema, out var appTheme))
        {
            TemaSeleccionado = tema;
            if (Application.Current != null)
            {
                MainThread.BeginInvokeOnMainThread(() => Application.Current.UserAppTheme = appTheme);
            }
        }
    }

    [RelayCommand]
    private async Task Logout()
    {
        bool confirmar = await Shell.Current.DisplayAlert(
            "Cerrar sesión",
            "¿Estás seguro de que quieres cerrar sesión?",
            "Sí, salir",
            "Cancelar");

        if (!confirmar)
            return;

        _userService.Logout();

        if (Application.Current != null)
        {
            Application.Current.Windows[0].Page = new NavigationPage(
                Application.Current.Handler.MauiContext.Services.GetRequiredService<GestionTareas.View.LoginView>());
        }
    }

    [RelayCommand]
    private async Task EditarPerfil()
    {
        await Shell.Current.GoToAsync("EditarPerfilView");
    }

    public async Task RefreshUserData()
    {
        await LoadUserData();
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
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    UsuarioActual = usuario;
                    NombreUsuario = usuario.Nombre ?? "No disponible";
                    EmailUsuario = usuario.Email ?? "No disponible";
                    IdUsuario = $"ID: {usuario.Id}";
                    RolGlobal = usuario.Rol?.Nombre ?? "No disponible";

                    // Generar iniciales del nombre
                    if (!string.IsNullOrEmpty(usuario.Nombre))
                    {
                        var palabras = usuario.Nombre.Trim().Split(' ');
                        if (palabras.Length >= 2)
                        {
                            InicialesUsuario = $"{palabras[0][0]}{palabras[1][0]}".ToUpper();
                        }
                        else if (palabras.Length == 1 && palabras[0].Length >= 2)
                        {
                            InicialesUsuario = palabras[0].Substring(0, 2).ToUpper();
                        }
                        else
                        {
                            InicialesUsuario = palabras[0][0].ToString().ToUpper();
                        }
                    }
                    else
                    {
                        InicialesUsuario = "??";
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No se pudo obtener el usuario actual");
                MainThread.BeginInvokeOnMainThread(() => 
                {
                    NombreUsuario = "No disponible";
                    EmailUsuario = "No disponible";
                    IdUsuario = "No disponible";
                    RolGlobal = "No disponible";
                    InicialesUsuario = "??";
                });
            }
        }
        catch (Exception ex) 
        { 
            System.Diagnostics.Debug.WriteLine($"❌ Error en LoadUserData: {ex.Message}");
            MainThread.BeginInvokeOnMainThread(() => 
            {
                NombreUsuario = "No disponible";
                EmailUsuario = "No disponible";
                IdUsuario = "No disponible";
                RolGlobal = "No disponible";
                InicialesUsuario = "??";
            });
        }
    }
}
