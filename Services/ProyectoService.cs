using GestionTareas.Model;
using System.Diagnostics;
using System.Text.Json;
namespace GestionTareas.Services;

public class ProyectoService : IRestService<Proyecto>
{
    HttpClient _client = new();
    JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

    Uri uri = new Uri(string.Format($"{ApiConfig.BaseUrl}/proyectos"));
    

    public async Task<List<Proyecto>> GetAllAsync()
    {
        var items = new List<Proyecto>();

        try
        {
            HttpResponseMessage response = await _client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Proyecto>>(content, _jsonSerializerOptions);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"\tERROR {0}", ex.Message);
        }

        return items;
        //var departments = await _client.GetFromJsonAsync<List<Department>>(uri + "/Deparments/", _jsonSerializerOptions);
        //var asd = 0;
        //return departments;
    }
}

