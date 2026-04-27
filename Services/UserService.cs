using GestionTareas.Model;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GestionTareas.Services;

public class UserService : IRestService<Usuario>
{
    HttpClient _client = new();

    // Usamos CamelCase para que coincida con ProyectoService
    JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // La URL base viene de tu clase ApiConfig
    Uri uri = new Uri($"{ApiConfig.BaseUrl}/usuarios");

    public async Task<List<Usuario>> GetAllAsync()
    {
        var items = new List<Usuario>();

        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Usuario>>(content, _jsonSerializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR GetAllAsync: {0}", ex.Message);
        }

        return items;
    }

    public async Task<bool> CreateAsync(Usuario usuario)
    {
        try
        {
            var json = JsonSerializer.Serialize(usuario, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"ERROR CreateAsync: {0}", ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var deleteUri = new Uri($"{ApiConfig.BaseUrl}/usuarios/{id}");
            HttpResponseMessage response = await _client.DeleteAsync(deleteUri);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"ERROR DeleteAsync: {0}", ex.Message);
            return false;
        }
    }

    // Método extra para el Login (necesario para tu ViewModel)
    public async Task<Usuario> LoginAsync(string email, string password)
    {
        try
        {
            var usuarios = await GetAllAsync();
            return usuarios.FirstOrDefault(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"ERROR LoginAsync: {0}", ex.Message);
            return null;
        }
    }
}