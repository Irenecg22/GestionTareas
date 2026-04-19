using GestionTareas.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace GestionTareas.Services
{
    public class UserService : IRestService<Usuario>
    {
        HttpClient _client = new();
        JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, };

        Uri uri = new Uri(string.Format("http://127.0.0.1:8000/usuarios"));

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
                Debug.WriteLine(@"\tERROR {0}", ex.Message);
            }

            return items;
        }
    }
}
