using GestionTareas.Model;
using System.Diagnostics;
using System.Text;
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

    public async Task<bool> CreateAsync(Proyecto proyecto)
    {
        try
        {
            var json = JsonSerializer.Serialize(proyecto, _jsonSerializerOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(uri, content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"ERROR {0}", ex.Message);
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var deleteUri = new Uri($"{ApiConfig.BaseUrl}/proyectos/{id}");
            HttpResponseMessage response = await _client.DeleteAsync(deleteUri);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(@"ERROR {0}", ex.Message);
            return false;
        }
    }
}

