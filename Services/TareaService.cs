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
        _client.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Tarea>> GetAllAsync()
    {
        var items = new List<Tarea>();

        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync(uri);

            Debug.WriteLine($"📝 TareaService.GetAllAsync - Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Tarea>>(content, _options) ?? new();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Error GetAllAsync Tareas: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR GetAllAsync Tareas: {ex.Message}");
        }

        return items;
    }

    public async Task<(bool Success, int StatusCode)> CreateWithStatusAsync(Tarea tarea)
    {
        try
        {
            await AddAuthHeaderAsync();

            var json = JsonSerializer.Serialize(tarea);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, content);

            Debug.WriteLine($"📝 CreateAsync Tarea - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR CreateAsync Tarea: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<bool> CreateAsync(Tarea tarea)
    {
        var (success, _) = await CreateWithStatusAsync(tarea);
        return success;
    }

    public async Task<(bool Success, int StatusCode)> DeleteWithStatusAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.DeleteAsync(new Uri($"{ApiConfig.BaseUrl}/tareas/{id}"));

            Debug.WriteLine($"📝 DeleteAsync Tarea - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR DeleteAsync Tarea: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var (success, _) = await DeleteWithStatusAsync(id);
        return success;
    }

    public async Task<(bool Success, int StatusCode)> UpdateAsync(int id, Tarea tarea)
    {
        try
        {
            await AddAuthHeaderAsync();

            var json = JsonSerializer.Serialize(tarea);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PatchAsync(new Uri($"{ApiConfig.BaseUrl}/tareas/{id}"), content);

            Debug.WriteLine($"📝 UpdateAsync Tarea - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR UpdateAsync Tarea: {ex.Message}");
            return (false, 0);
        }
    }
}