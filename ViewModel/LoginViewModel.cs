using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Services;
using GestionTareas.View;
using Microsoft.Extensions.DependencyInjection;
namespace GestionTareas.ViewModel;
public partial class LoginViewModel : ObservableObject
{
    private readonly UserService _userService;
    [ObservableProperty]
    private string email;
    [ObservableProperty]
    private string password;
    [ObservableProperty]
    private bool isLoading;
    public LoginViewModel(UserService userService)
    {
        _userService = userService;
    }
    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                "Introduce email y contraseña.",
                "OK");
            return;
        }
        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"?? Intentando login desde UI con: {Email}");
            bool loginCorrecto = await _userService.LoginAsync(Email, Password);
            if (!loginCorrecto)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error de autenticación",
                    "Las credenciales no son correctas.\n\n" +
                    "Verifica:\n" +
                    "• Email correcto (ejemplo: juan@test.com)\n" +
                    "• Contraseña correcta\n" +
                    "• Que el backend esté ejecutándose\n\n" +
                    "Revisa la ventana Output > Debug para más detalles.",
                    "OK");
                return;
            }
            System.Diagnostics.Debug.WriteLine($"? Login exitoso, cambiando a AppShell");
            Application.Current.Windows[0].Page = new AppShell();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"? Excepción en Login UI: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert(
                "Error",
                $"Error inesperado: {ex.Message}",
                "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
    [RelayCommand]
    private async Task GoToRegister()
    {
        await Application.Current.MainPage.Navigation.PushAsync(
            App.Current.Handler.MauiContext.Services.GetRequiredService<SignUpView>());
    }
}