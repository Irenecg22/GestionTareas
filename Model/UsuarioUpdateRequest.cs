using System.Text.Json.Serialization;

namespace GestionTareas.Model;

/// <summary>
/// DTO para actualizar el perfil de usuario.
/// Permite modificar: nombre, email y contraseña (opcional).
/// Todos los campos son opcionales - solo se envían al backend los que se desean modificar.
/// </summary>
public class UsuarioUpdateRequest
{
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}

