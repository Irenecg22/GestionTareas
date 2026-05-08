using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
namespace GestionTareas.ViewModel;
public partial class EditarPerfilViewModel : ObservableObject
{
    private readonly UserService _userService;
    [ObservableProperty]
    private Usuario? usuarioActual;
    [ObservableProperty]
    private string nombre;
    [ObservableProperty]
    private string email;
    [ObservableProperty]
    private string? nuevaPassword;
    [ObservableProperty]
    private string? confirmarPassword;
    [ObservableProperty]
    private string? mensajeError;
    [ObservableProperty]
    private bool isLoading;
    public EditarPerfilViewModel(UserService userService)
    {
        _userService = userService;
        _ = LoadUserData();
    }
    private async Task LoadUserData()
    {
        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine("🔍 EditarPerfilViewModel: Cargando datos del usuario actual...");
            var usuario = await _userService.GetCurrentUserAsync();
            if (usuario != null)
            {
                System.Diagnostics.Debug.WriteLine($"✅ Usuario cargado: {usuario.Nombre} ({usuario.Email})");
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    UsuarioActual = usuario;
                    Nombre = usuario.Nombre ?? "";
                    Email = usuario.Email ?? "";
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No se pudo obtener el usuario actual");
                MensajeError = "No se pudo cargar la información del usuario.";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error en LoadUserData: {ex.Message}");
            MensajeError = "Error al cargar los datos del usuario.";
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task GuardarCambios()
    {
        MensajeError = null;
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El nombre es obligatorio.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(Email))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El email es obligatorio.", "OK");
            return;
        }
        if (!IsValidEmail(Email))
        {
            await Application.Current.MainPage.DisplayAlert("Error", "El formato del email no es válido.", "OK");
            return;
        }
        if (!string.IsNullOrWhiteSpace(NuevaPassword))
        {
            if (NuevaPassword.Length < 8)
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "La contraseña debe tener al menos 8 caracteres.", "OK");
                return;
            }
            if (NuevaPassword != ConfirmarPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Las contraseñas no coinciden.", "OK");
                return;
            }
        }
        if (UsuarioActual == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", 
                "No se pudo identificar al usuario actual.", "OK");
            return;
        }
        try
        {
            IsLoading = true;
            bool emailCambiado = Email != UsuarioActual.Email;
            bool passwordCambiado = !string.IsNullOrWhiteSpace(NuevaPassword);
            var request = new UsuarioUpdateRequest
            {
                Nombre = Nombre,
                Email = Email,
                Password = passwordCambiado ? NuevaPassword : null
            };
            var (success, statusCode, errorMessage) = await _userService.UpdateUserAsync(UsuarioActual.Id, request);
            if (success)
            {
                if (emailCambiado || passwordCambiado)
                {
                    string mensaje = "Perfil actualizado correctamente.\n\n";
                    if (emailCambiado && passwordCambiado)
                    {
                        mensaje += "Has cambiado tu email y contraseña. Por seguridad, debes iniciar sesión nuevamente con tus nuevas credenciales.";
                    }
                    else if (emailCambiado)
                    {
                        mensaje += "Has cambiado tu email. Por seguridad, debes iniciar sesión nuevamente con tu nuevo email.";
                    }
                    else
                    {
                        mensaje += "Has cambiado tu contraseña. Por seguridad, debes iniciar sesión nuevamente.";
                    }
                    await Application.Current.MainPage.DisplayAlert("Actualización exitosa", mensaje, "OK");
                    _userService.Logout();
                    if (Application.Current != null)
                    {
                        Application.Current.Windows[0].Page = new NavigationPage(
                            Application.Current.Handler.MauiContext.Services.GetRequiredService<GestionTareas.View.LoginView>());
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", 
                        "Perfil actualizado correctamente.", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            else if (statusCode == 401)
            {
                await Application.Current.MainPage.DisplayAlert("Sesión expirada",
                    "Tu sesión ha expirado. Vuelve a iniciar sesión.", "OK");
            }
            else if (statusCode == 403)
            {
                await Application.Current.MainPage.DisplayAlert("Sin permisos",
                    "No tienes permisos para modificar este usuario.", "OK");
            }
            else if (statusCode == 400)
            {
                await Application.Current.MainPage.DisplayAlert("Error de validación",
                    errorMessage ?? "Los datos proporcionados no son válidos.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error",
                    errorMessage ?? "No se pudo actualizar el perfil.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"❌ Error al guardar cambios: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error",
                "Ocurrió un error al actualizar el perfil.", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task Cancelar()
    {
        await Shell.Current.GoToAsync("..");
    }
    private bool IsValidEmail(string email)
    {
        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}
