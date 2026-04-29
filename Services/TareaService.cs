using GestionTareas.Model;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GestionTareas.Services;

public class TareaService : IRestService<Tarea>
{
    private readonly HttpClient _client = new();
    private readonly UserService _userService;

    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Uri uri = new($"{ApiConfig.BaseUrl}/tareas/");

    public TareaService(UserService userService)
    {
        _userService = userService;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _userService.GetTokenAsync();

        Debug.WriteLine($"🔐 TareaService - AddAuthHeaderAsync");
        Debug.WriteLine($"   Token obtenido: {(string.IsNullOrWhiteSpace(token) ? "❌ VACÍO O NULO" : $"✅ {token.Substring(0, Math.Min(20, token.Length))}...")}");

        _client.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            Debug.WriteLine($"   ✅ Authorization header configurado correctamente");
        }
        else
        {
            Debug.WriteLine($"   ⚠️ No hay token disponible - petición SIN autenticación");
        }
    }

    public async Task<List<Tarea>> GetAllAsync()
    {
        var items = new List<Tarea>();

        try
        {
            Debug.WriteLine($"📝 TareaService.GetAllAsync - Iniciando...");
            Debug.WriteLine($"   URL: {uri}");

            await AddAuthHeaderAsync();

            Debug.WriteLine($"   🚀 Enviando petición GET...");
            var response = await _client.GetAsync(uri);

            Debug.WriteLine($"   📬 Status Code: {response.StatusCode} ({(int)response.StatusCode})");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"   ✅ Respuesta exitosa. Longitud: {content.Length} caracteres");

                items = JsonSerializer.Deserialize<List<Tarea>>(content, _options) ?? new();
                Debug.WriteLine($"   ✅ Tareas deserializadas: {items.Count}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"   ❌ Error del servidor:");
                Debug.WriteLine($"      Status: {response.StatusCode}");
                Debug.WriteLine($"      Contenido: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR GetAllAsync Tareas:");
            Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            Debug.WriteLine($"   Mensaje: {ex.Message}");
            Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
        }

        Debug.WriteLine($"📝 TareaService.GetAllAsync - Retornando {items.Count} tareas");
        return items;
    }

    public async Task<bool> CreateAsync(Tarea tarea)
    {
        try
        {
            await AddAuthHeaderAsync();

            var json = JsonSerializer.Serialize(tarea);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR CreateAsync Tarea: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();

            var deleteUri = new Uri($"{ApiConfig.BaseUrl}/tareas/{id}");
            var response = await _client.DeleteAsync(deleteUri);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR DeleteAsync Tarea: {ex.Message}");
            return false;
        }
    }
}