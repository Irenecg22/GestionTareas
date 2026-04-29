using GestionTareas.Model;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GestionTareas.Services;

public class ProyectoService : IRestService<Proyecto>
{
    private readonly HttpClient _client = new();
    private readonly UserService _userService;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly Uri uri = new($"{ApiConfig.BaseUrl}/proyectos/");

    public ProyectoService(UserService userService)
    {
        _userService = userService;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = await _userService.GetTokenAsync();

        Debug.WriteLine($"🔐 ProyectoService - AddAuthHeaderAsync");
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

    public async Task<List<Proyecto>> GetAllAsync()
    {
        var items = new List<Proyecto>();

        try
        {
            Debug.WriteLine($"📋 ProyectoService.GetAllAsync - Iniciando...");
            Debug.WriteLine($"   URL: {uri}");

            await AddAuthHeaderAsync();

            Debug.WriteLine($"   🚀 Enviando petición GET...");
            var response = await _client.GetAsync(uri);

            Debug.WriteLine($"   📬 Status Code: {response.StatusCode} ({(int)response.StatusCode})");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"   ✅ Respuesta exitosa. Longitud: {content.Length} caracteres");
                Debug.WriteLine($"   Contenido: {content.Substring(0, Math.Min(200, content.Length))}...");

                items = JsonSerializer.Deserialize<List<Proyecto>>(content, _jsonOptions) ?? new();
                Debug.WriteLine($"   ✅ Proyectos deserializados: {items.Count}");
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
            Debug.WriteLine($"❌ ERROR GetAllAsync Proyectos:");
            Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            Debug.WriteLine($"   Mensaje: {ex.Message}");
            Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
        }

        Debug.WriteLine($"📋 ProyectoService.GetAllAsync - Retornando {items.Count} proyectos");
        return items;
    }

    public async Task<bool> CreateAsync(Proyecto proyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            var json = JsonSerializer.Serialize(proyecto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR CreateAsync Proyecto: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();

            var deleteUri = new Uri($"{ApiConfig.BaseUrl}/proyectos/{id}");
            var response = await _client.DeleteAsync(deleteUri);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR DeleteAsync Proyecto: {ex.Message}");
            return false;
        }
    }
}