using GestionTareas.Model;
using System.Diagnostics;
using System.Text.Json;

namespace GestionTareas.Services;

public class TareaService : IRestService<Tarea>
{
    HttpClient _client = new();
    JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    Uri uri = new Uri(string.Format($"{ApiConfig.BaseUrl}/tareas"));

    public async Task<List<Tarea>> GetAllAsync()
    {
        var items = new List<Tarea>();

        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Tarea>>(content, _jsonSerializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return items;
    }
    public Task<bool> DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

}
