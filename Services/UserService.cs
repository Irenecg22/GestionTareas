using GestionTareas.Model;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionTareas.Services;

public class UserService : IRestService<Usuario>
{
    private readonly HttpClient _client = new();

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginUrl = $"{ApiConfig.BaseUrl}/auth/login";

            Debug.WriteLine($"🔐 Intentando login con: {email}");
            Debug.WriteLine($"📡 URL: {loginUrl}");

            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", email),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await _client.PostAsync(loginUrl, formData);

            Debug.WriteLine($"📬 Status Code: {response.StatusCode}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Error del servidor: {errorContent}");
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"✅ Respuesta del servidor: {json}");

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(json, _jsonOptions);

            if (string.IsNullOrWhiteSpace(tokenResponse?.AccessToken))
            {
                Debug.WriteLine($"⚠️ Token vacío o nulo en la respuesta");
                return false;
            }

            Debug.WriteLine($"🎟️ Token recibido: {tokenResponse.AccessToken.Substring(0, 20)}...");

            await SecureStorage.SetAsync("access_token", tokenResponse.AccessToken);

            Debug.WriteLine($"💾 Token guardado exitosamente");

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR LoginAsync: {ex.Message}");
            Debug.WriteLine($"📋 StackTrace: {ex.StackTrace}");
            return false;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await SecureStorage.GetAsync("access_token");
        }
        catch
        {
            return null;
        }
    }

    public void Logout()
    {
        SecureStorage.Remove("access_token");
    }

    public async Task<bool> CreateAsync(Usuario usuario)
    {
        try
        {
            var registerUrl = $"{ApiConfig.BaseUrl}/auth/register";

            Debug.WriteLine($"📝 ===== USER SERVICE: CREATE ASYNC =====");
            Debug.WriteLine($"   URL: {registerUrl}");
            Debug.WriteLine($"   Nombre: {usuario.Nombre}");
            Debug.WriteLine($"   Email: {usuario.Email}");
            Debug.WriteLine($"   Password Length: {usuario.Password?.Length ?? 0}");
            Debug.WriteLine($"   RolId: {usuario.RolId}");

            var request = new RegisterRequest
            {
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                Password = usuario.Password,
                RolId = usuario.RolId
            };

            var json = JsonSerializer.Serialize(request);
            Debug.WriteLine($"📤 JSON Request: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Debug.WriteLine($"⏳ Enviando petición al backend...");
            var response = await _client.PostAsync(registerUrl, content);

            Debug.WriteLine($"📬 Status Code: {response.StatusCode} ({(int)response.StatusCode})");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"❌ Error del servidor:");
                Debug.WriteLine($"   Status: {response.StatusCode}");
                Debug.WriteLine($"   Content: {errorContent}");
                return false;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"✅ Respuesta exitosa del servidor:");
            Debug.WriteLine($"   Content: {responseContent}");

            return true;
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"❌ ERROR HTTP en CreateAsync:");
            Debug.WriteLine($"   Mensaje: {httpEx.Message}");
            Debug.WriteLine($"   ¿Backend corriendo? Verifica http://localhost:8000/docs");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR en CreateAsync:");
            Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            Debug.WriteLine($"   Mensaje: {ex.Message}");
            Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            return false;
        }
        finally
        {
            Debug.WriteLine($"📝 ===== FIN CREATE ASYNC =====");
        }
    }

    public async Task<List<Usuario>> GetAllAsync()
    {
        var items = new List<Usuario>();

        try
        {
            var token = await GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.GetAsync($"{ApiConfig.BaseUrl}/usuarios/");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                items = JsonSerializer.Deserialize<List<Usuario>>(content, _jsonOptions) ?? new();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR GetAllAsync: {ex.Message}");
        }

        return items;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var token = await GetTokenAsync();

            if (!string.IsNullOrWhiteSpace(token))
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync($"{ApiConfig.BaseUrl}/usuarios/{id}");

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR DeleteAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<Usuario?> GetCurrentUserAsync()
    {
        try
        {
            var token = await GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                Debug.WriteLine($"⚠️ GetCurrentUserAsync: No hay token disponible");
                return null;
            }

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Usar /auth/me como fuente principal de verdad
            Debug.WriteLine($"👤 Obteniendo usuario actual desde: {ApiConfig.BaseUrl}/auth/me");

            var response = await _client.GetAsync($"{ApiConfig.BaseUrl}/auth/me");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✅ Usuario actual obtenido desde /auth/me");

                var usuario = JsonSerializer.Deserialize<Usuario>(content, _jsonOptions);

                if (usuario != null)
                {
                    Debug.WriteLine($"   ID: {usuario.Id}");
                    Debug.WriteLine($"   Nombre: {usuario.Nombre}");
                    Debug.WriteLine($"   Email: {usuario.Email}");
                }

                return usuario;
            }

            Debug.WriteLine($"❌ /auth/me falló con código: {response.StatusCode}");

            // Si el token es inválido o expiró, limpiar el token guardado
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Debug.WriteLine($"⚠️ Token expirado o inválido, limpiando token de SecureStorage");
                Logout();
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR GetCurrentUserAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<(bool Success, int StatusCode, string? Message)> UpdateUserAsync(int userId, UsuarioUpdateRequest request)
    {
        try
        {
            var token = await GetTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                Debug.WriteLine($"⚠️ UpdateUserAsync: No hay token disponible");
                return (false, 401, "No autenticado");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var updateUrl = $"{ApiConfig.BaseUrl}/usuarios/{userId}";
            Debug.WriteLine($"✏️ ===== ACTUALIZANDO USUARIO =====");
            Debug.WriteLine($"   URL: {updateUrl}");
            Debug.WriteLine($"   User ID: {userId}");
            Debug.WriteLine($"   Nombre: {request.Nombre ?? "(no cambiar)"}");
            Debug.WriteLine($"   Email: {request.Email ?? "(no cambiar)"}");
            Debug.WriteLine($"   Password: {(string.IsNullOrEmpty(request.Password) ? "(no cambiar)" : "***********")}");

            // Serializar solo los campos no nulos
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(request, options);
            Debug.WriteLine($"📤 JSON Request: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            Debug.WriteLine($"⏳ Enviando PATCH request...");
            var response = await _client.PatchAsync(updateUrl, content);

            Debug.WriteLine($"📬 Status Code: {response.StatusCode} ({(int)response.StatusCode})");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✅ Usuario actualizado correctamente");
                Debug.WriteLine($"   Response: {responseContent}");
                Debug.WriteLine($"✏️ ===== FIN ACTUALIZACIÓN =====");
                return (true, (int)response.StatusCode, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error actualizando usuario");
            Debug.WriteLine($"   Status: {response.StatusCode}");
            Debug.WriteLine($"   Content: {errorContent}");

            // Intentar extraer mensaje de error del backend
            string? errorMessage = null;
            try
            {
                using var document = JsonDocument.Parse(errorContent);
                if (document.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    errorMessage = detailElement.GetString();
                    Debug.WriteLine($"   Detail: {errorMessage}");
                }
            }
            catch
            {
                errorMessage = errorContent;
            }

            Debug.WriteLine($"✏️ ===== FIN ACTUALIZACIÓN (ERROR) =====");
            return (false, (int)response.StatusCode, errorMessage);
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"❌ ERROR HTTP en UpdateUserAsync:");
            Debug.WriteLine($"   Mensaje: {httpEx.Message}");
            Debug.WriteLine($"   ¿Backend corriendo? Verifica {ApiConfig.BaseUrl}");
            return (false, 0, $"Error de conexión: {httpEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR UpdateUserAsync:");
            Debug.WriteLine($"   Tipo: {ex.GetType().Name}");
            Debug.WriteLine($"   Mensaje: {ex.Message}");
            Debug.WriteLine($"   StackTrace: {ex.StackTrace}");
            return (false, 0, ex.Message);
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }
    }

    private class RegisterRequest
    {
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("id_rol")]
        public int RolId { get; set; }
    }
}