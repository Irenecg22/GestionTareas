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
        _client.DefaultRequestHeaders.Authorization = null;

        if (!string.IsNullOrWhiteSpace(token))
        {
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    public async Task<List<Proyecto>> GetAllAsync()
    {
        var items = new List<Proyecto>();

        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync(uri);

            Debug.WriteLine($" ProyectoService.GetAllAsync - Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Proyecto>>(content, _jsonOptions) ?? new();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($" Error GetAllAsync: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR GetAllAsync Proyectos: {ex.Message}");
        }

        return items;
    }

    public async Task<Proyecto?> GetByIdAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync(new Uri($"{ApiConfig.BaseUrl}/proyectos/{id}"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Proyecto>(content, _jsonOptions);
            }

            Debug.WriteLine($" GetByIdAsync: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR GetByIdAsync: {ex.Message}");
        }

        return null;
    }

    public async Task<bool> CreateAsync(Proyecto proyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            // Solo enviar nombre, descripcion, fecha_creacion (NO creado_por_id)
            var payload = new
            {
                nombre = proyecto.Nombre,
                descripcion = proyecto.Descripcion,
                fecha_creacion = proyecto.FechaCreacion
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(uri, content);

            Debug.WriteLine($" CreateAsync Proyecto - Status: {response.StatusCode}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR CreateAsync Proyecto: {ex.Message}");
            return false;
        }
    }

    public async Task<(bool Success, int StatusCode)> UpdateAsync(int id, Proyecto proyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            var payload = new
            {
                nombre = proyecto.Nombre,
                descripcion = proyecto.Descripcion,
                fecha_creacion = proyecto.FechaCreacion
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, new Uri($"{ApiConfig.BaseUrl}/proyectos/{id}"))
            {
                Content = content
            };

            var response = await _client.SendAsync(request);
            Debug.WriteLine($" UpdateAsync Proyecto - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR UpdateAsync Proyecto: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<(bool Success, int StatusCode)> DeleteWithStatusAsync(int id)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.DeleteAsync(new Uri($"{ApiConfig.BaseUrl}/proyectos/{id}"));
            Debug.WriteLine($" DeleteAsync Proyecto - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR DeleteAsync Proyecto: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var (success, _) = await DeleteWithStatusAsync(id);
        return success;
    }


    public async Task<List<ProyectoUsuario>> GetMiembrosAsync(int proyectoId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.GetAsync(new Uri($"{ApiConfig.BaseUrl}/proyectos/{proyectoId}/miembros"));

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<ProyectoUsuario>>(content, _jsonOptions) ?? new();
            }

            Debug.WriteLine($" GetMiembrosAsync: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR GetMiembrosAsync: {ex.Message}");
        }

        return new();
    }

    public async Task<(bool Success, int StatusCode)> AddMiembroAsync(int proyectoId, int usuarioId, string rolProyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            var payload = new ProyectoUsuarioCreateRequest
            {
                IdUsuario = usuarioId,
                RolProyecto = rolProyecto
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(
                new Uri($"{ApiConfig.BaseUrl}/proyectos/{proyectoId}/miembros"), content);

            Debug.WriteLine($" AddMiembroAsync - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR AddMiembroAsync: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<(bool Success, int StatusCode)> UpdateMiembroRolAsync(int proyectoId, int usuarioId, string rolProyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            var payload = new ProyectoUsuarioUpdateRequest { RolProyecto = rolProyecto };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch,
                new Uri($"{ApiConfig.BaseUrl}/proyectos/{proyectoId}/miembros/{usuarioId}"))
            {
                Content = content
            };

            var response = await _client.SendAsync(request);
            Debug.WriteLine($" UpdateMiembroRolAsync - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR UpdateMiembroRolAsync: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<(bool Success, int StatusCode)> RemoveMiembroAsync(int proyectoId, int usuarioId)
    {
        try
        {
            await AddAuthHeaderAsync();
            var response = await _client.DeleteAsync(
                new Uri($"{ApiConfig.BaseUrl}/proyectos/{proyectoId}/miembros/{usuarioId}"));

            Debug.WriteLine($" RemoveMiembroAsync - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR RemoveMiembroAsync: {ex.Message}");
            return (false, 0);
        }
    }

    public async Task<(bool Success, int StatusCode)> AddMiembroPorEmailAsync(int proyectoId, string email, string rolProyecto)
    {
        try
        {
            await AddAuthHeaderAsync();

            var payload = new ProyectoUsuarioEmailCreateRequest
            {
                Email = email,
                RolProyecto = rolProyecto
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(
                new Uri($"{ApiConfig.BaseUrl}/proyectos/{proyectoId}/miembros/email"), content);

            Debug.WriteLine($" AddMiembroPorEmailAsync - Status: {response.StatusCode}");
            return (response.IsSuccessStatusCode, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($" ERROR AddMiembroPorEmailAsync: {ex.Message}");
            return (false, 0);
        }
    }
}