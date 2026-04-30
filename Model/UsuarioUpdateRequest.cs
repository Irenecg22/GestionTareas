using System.Text.Json.Serialization;

namespace GestionTareas.Model;

public class UsuarioUpdateRequest
{
    [JsonPropertyName("nombre")]
    public string? Nombre { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("password")]
    public string? Password { get; set; }
}
