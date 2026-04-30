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

            // Intentar primero /auth/me
            Debug.WriteLine($"👤 Intentando obtener usuario actual desde: {ApiConfig.BaseUrl}/auth/me");

            var response = await _client.GetAsync($"{ApiConfig.BaseUrl}/auth/me");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✅ Usuario actual obtenido desde /auth/me: {content}");

                var usuario = JsonSerializer.Deserialize<Usuario>(content, _jsonOptions);
                return usuario;
            }

            Debug.WriteLine($"⚠️ /auth/me no disponible ({response.StatusCode}), intentando método alternativo...");

            // Método alternativo: decodificar el email del token JWT y buscar por email
            var userEmail = GetEmailFromToken(token);
            if (!string.IsNullOrWhiteSpace(userEmail))
            {
                Debug.WriteLine($"📧 Email extraído del token: {userEmail}");
                Debug.WriteLine($"🔍 Buscando usuario por email en: {ApiConfig.BaseUrl}/usuarios");

                var usuarios = await GetAllAsync();
                var usuarioActual = usuarios?.FirstOrDefault(u => 
                    u.Email.Equals(userEmail, StringComparison.OrdinalIgnoreCase));

                if (usuarioActual != null)
                {
                    Debug.WriteLine($"✅ Usuario encontrado: {usuarioActual.Nombre} ({usuarioActual.Email})");
                    return usuarioActual;
                }

                Debug.WriteLine($"❌ No se encontró usuario con email: {userEmail}");
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR GetCurrentUserAsync: {ex.Message}");
            return null;
        }
    }

    private string? GetEmailFromToken(string token)
    {
        try
        {
            // JWT token tiene formato: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
                return null;

            // Decodificar el payload (segunda parte)
            var payload = parts[1];

            // Ajustar padding para Base64
            var base64 = payload.Replace('-', '+').Replace('_', '/');
            while (base64.Length % 4 != 0)
                base64 += "=";

            var jsonBytes = Convert.FromBase64String(base64);
            var json = Encoding.UTF8.GetString(jsonBytes);

            Debug.WriteLine($"🔓 Token payload decodificado: {json}");

            // Parsear JSON para obtener el "sub" (subject) que generalmente contiene el email
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty("sub", out var subElement))
            {
                return subElement.GetString();
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"⚠️ Error decodificando token: {ex.Message}");
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
            Debug.WriteLine($"✏️ Actualizando usuario en: {updateUrl}");
            Debug.WriteLine($"   Nombre: {request.Nombre}");
            Debug.WriteLine($"   Email: {request.Email}");
            Debug.WriteLine($"   Contraseña: {(string.IsNullOrEmpty(request.Password) ? "No cambiar" : "Actualizar")}");

            // Serializar solo los campos no nulos
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            var json = JsonSerializer.Serialize(request, options);
            Debug.WriteLine($"📤 JSON Request: {json}");

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PatchAsync(updateUrl, content);

            Debug.WriteLine($"📬 Status Code: {response.StatusCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"✅ Usuario actualizado correctamente: {responseContent}");
                return (true, (int)response.StatusCode, null);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"❌ Error actualizando usuario: {errorContent}");

            // Intentar extraer mensaje de error del backend
            string? errorMessage = null;
            try
            {
                using var document = JsonDocument.Parse(errorContent);
                if (document.RootElement.TryGetProperty("detail", out var detailElement))
                {
                    errorMessage = detailElement.GetString();
                }
            }
            catch
            {
                errorMessage = errorContent;
            }

            return (false, (int)response.StatusCode, errorMessage);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ ERROR UpdateUserAsync: {ex.Message}");
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