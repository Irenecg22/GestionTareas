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
            if (string.IsNullOrWhiteSpace(Nombre) ||
                string.IsNullOrWhiteSpace(Email) ||
                string.IsNullOrWhiteSpace(Password))
            {
                await Shell.Current.DisplayAlert("Error", "Por favor, rellena todos los campos.", "OK");
                return;
            }

            if (Password != ConfirmPassword)
            {
                await Shell.Current.DisplayAlert("Error", "Las contraseñas no coinciden.", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                var nuevoUsuario = new Usuario
                {
                    Nombre = Nombre,
                    Email = Email,
                    Password = Password,
                    RolId = 2 
                };

                bool exito = await _userService.CreateAsync(nuevoUsuario);

                if (exito)
                {
                    await Shell.Current.DisplayAlert("Éxito", "Cuenta creada correctamente", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Error", "No se pudo crear la cuenta. Revisa la conexión.", "OK");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en Register: {ex.Message}");
                await Shell.Current.DisplayAlert("Error", "Ocurrió un error inesperado.", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}