using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace GestionTareas.ViewModel
{
    public partial class SignUpViewModel : ObservableObject
    {
        private readonly UserService _userService;
        [ObservableProperty]
        private string nombre;
        [ObservableProperty]
        private string email;
        [ObservableProperty]
        private string password;
        [ObservableProperty]
        private string confirmPassword;
        [ObservableProperty]
        private bool isLoading;
        [ObservableProperty]
        private bool hasError;
        [ObservableProperty]
        private string errorMessage;
        public SignUpViewModel(UserService userService)
        {
            _userService = userService;
        }
        private void ShowError(string message)
        {
            ErrorMessage = message;
            HasError = true;
        }
        private void ClearError()
        {
            ErrorMessage = string.Empty;
            HasError = false;
        }
        private bool ValidateInputs()
        {
            ClearError();
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                ShowError("? El nombre es obligatorio.");
                return false;
            }
            if (Nombre.Length < 3)
            {
                ShowError("? El nombre debe tener al menos 3 caracteres.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Email))
            {
                ShowError("? El email es obligatorio.");
                return false;
            }
            var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!emailRegex.IsMatch(Email))
            {
                ShowError("? El formato del email no es válido.\nEjemplo: usuario@ejemplo.com");
                return false;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ShowError("? La contraseña es obligatoria.");
                return false;
            }
            if (Password.Length < 6)
            {
                ShowError("? La contraseña debe tener al menos 8 caracteres.");
                return false;
            }
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ShowError("? Debes confirmar tu contraseña.");
                return false;
            }
            if (Password != ConfirmPassword)
            {
                ShowError("? Las contraseñas no coinciden.\nPor favor, verifica que ambas sean iguales.");
                return false;
            }
            return true;
        }
        [RelayCommand]
        private async Task Register()
        {
            Debug.WriteLine("Registro");
            Debug.WriteLine($"   Nombre: {Nombre}");
            Debug.WriteLine($"   Email: {Email}");
            Debug.WriteLine($"   Password Length: {Password?.Length ?? 0}");
            Debug.WriteLine($"   ConfirmPassword Length: {ConfirmPassword?.Length ?? 0}");
            if (!ValidateInputs())
            {
                Debug.WriteLine(" Validación falló");
                return;
            }
            Debug.WriteLine($"? Validación local completada");
            try
            {
                IsLoading = true;
                ClearError();
                var nuevoUsuario = new Usuario
                {
                    Nombre = Nombre,
                    Email = Email,
                    Password = Password,
                    RolId = RolesConstantes.USUARIO  
                };
                Debug.WriteLine($"?? Llamando a UserService.CreateAsync...");
                Debug.WriteLine($"   Usuario: {nuevoUsuario.Nombre}");
                Debug.WriteLine($"   Email: {nuevoUsuario.Email}");
                Debug.WriteLine($"   RolId: {nuevoUsuario.RolId} ({RolesConstantes.ObtenerNombreRol(nuevoUsuario.RolId)})");
                bool exito = await _userService.CreateAsync(nuevoUsuario);
                Debug.WriteLine($"?? Respuesta de UserService: {(exito ? "ÉXITO" : "FALLO")}");
                if (exito)
                {
                    Debug.WriteLine($"?? Cuenta creada exitosamente");
                    await Application.Current.MainPage.DisplayAlert(
                        "? Registro Exitoso",
                        "Tu cuenta ha sido creada correctamente.\nAhora puedes iniciar sesión con tus credenciales.",
                        "Entendido");
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    Debug.WriteLine($"?? No se pudo crear la cuenta (servicio retornó false)");
                    ShowError("? No se pudo completar el registro.\n\n" +
                             "Posibles causas:\n" +
                             "• El email ya está registrado\n" +
                             "• Error de conexión con el servidor\n" +
                             "• Datos no válidos\n\n" +
                             "Por favor, intenta con otro email o verifica tu conexión.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Debug.WriteLine($"? ERROR HTTP en Register:");
                Debug.WriteLine($"   Mensaje: {httpEx.Message}");
                ShowError("? Error de conexión con el servidor.\n\n" +
                         "Verifica que:\n" +
                         "• Estés conectado a internet\n" +
                         "• El servidor esté disponible\n\n" +
                         $"Detalle técnico: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"? EXCEPCIÓN en Register:");
                Debug.WriteLine($"   Mensaje: {ex.Message}");
                Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
                ShowError($"? Error inesperado durante el registro.\n\n" +
                         $"Detalle: {ex.Message}\n\n" +
                         "Por favor, intenta nuevamente.");
            }
            finally
            {
                IsLoading = false;
            }
        }
        [RelayCommand]
        private async Task GoToLogin()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}