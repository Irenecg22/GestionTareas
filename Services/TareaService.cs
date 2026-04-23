using GestionTareas.Model;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GestionTareas.Services;

public class TareaService : IRestService<Tarea>
{
    HttpClient _client = new();
    JsonSerializerOptions _options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    Uri uri = new Uri($"{ApiConfig.BaseUrl}/tareas");

    public async Task<List<Tarea>> GetAllAsync()
    {
        var items = new List<Tarea>();
        try
        {
            var response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Tarea>>(content, _options);
            }
        }
        catch (Exception ex) { Debug.WriteLine(@"ERROR {0}", ex.Message); }
        return items;
    }

    public async Task<bool> CreateAsync(Tarea tarea)
    {
        try
        {
            var json = JsonSerializer.Serialize(tarea, _options);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(uri, content);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Debug.WriteLine(@"ERROR {0}", ex.Message); return false; }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var deleteUri = new Uri($"{ApiConfig.BaseUrl}/tareas/{id}");
            var response = await _client.DeleteAsync(deleteUri);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex) { Debug.WriteLine(@"ERROR {0}", ex.Message); return false; }
    }
}