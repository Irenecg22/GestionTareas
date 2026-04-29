using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Model;
using GestionTareas.Services;
using System.Diagnostics;

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

        public SignUpViewModel(UserService userService)
        {
            _userService = userService;
        }

        [RelayCommand]
        private async Task Register()
        {
            Debug.WriteLine("📝 ===== INICIANDO PROCESO DE REGISTRO =====");
            Debug.WriteLine($"   Nombre: {Nombre}");
            Debug.WriteLine($"   Email: {Email}");
            Debug.WriteLine($"   Password Length: {Password?.Length ?? 0}");
            Debug.WriteLine($"   ConfirmPassword Length: {ConfirmPassword?.Length ?? 0}");

            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                Debug.WriteLine("❌ Error: Campos vacíos detectados");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Por favor, rellena todos los campos.",
                    "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                Debug.WriteLine("❌ Error: Las contraseñas no coinciden");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Las contraseñas no coinciden.",
                    "OK");
                return;
            }

            Debug.WriteLine($"✅ Validación local completada");

            try
            {
                IsLoading = true;

                var nuevoUsuario = new Usuario
                {
                    Nombre = Nombre,
                    Email = Email,
                    Password = Password,
                    RolId = 1
                };

                Debug.WriteLine($"📤 Llamando a UserService.CreateAsync...");
                Debug.WriteLine($"   Usuario: {nuevoUsuario.Nombre}");
                Debug.WriteLine($"   Email: {nuevoUsuario.Email}");
                Debug.WriteLine($"   RolId: {nuevoUsuario.RolId}");

                bool exito = await _userService.CreateAsync(nuevoUsuario);

                Debug.WriteLine($"📥 Respuesta de UserService: {(exito ? "ÉXITO" : "FALLO")}");

                if (exito)
                {
                    Debug.WriteLine($"🎉 Cuenta creada exitosamente");
                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Cuenta creada correctamente. Ahora puedes iniciar sesión.",
                        "OK");

                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    Debug.WriteLine($"⚠️ No se pudo crear la cuenta (servicio retornó false)");
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "No se pudo crear la cuenta. Revisa los datos.\n\n" +
                        "Verifica la ventana Output > Debug para más información.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ EXCEPCIÓN en Register:");
                Debug.WriteLine($"   Mensaje: {ex.Message}");
                Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
                Debug.WriteLine($"   StackTrace: {ex.StackTrace}");

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Ocurrió un error inesperado:\n{ex.Message}",
                    "OK");
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("📝 ===== FIN DEL PROCESO DE REGISTRO =====");
            }
        }

        [RelayCommand]
        private async Task GoToLogin()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}