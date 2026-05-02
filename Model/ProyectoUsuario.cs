using System.Text.Json.Serialization;

namespace GestionTareas.Model;

public class ProyectoUsuario
{
    [JsonPropertyName("id_proyecto_usuario")]
    public int IdProyectoUsuario { get; set; }

    [JsonPropertyName("id_proyecto")]
    public int IdProyecto { get; set; }

    [JsonPropertyName("id_usuario")]
    public int IdUsuario { get; set; }

    [JsonPropertyName("rol_proyecto")]
    public string RolProyecto { get; set; } = "viewer";

    /// <summary>
    /// Nombre del usuario (viene directamente del backend)
    /// </summary>
    [JsonPropertyName("nombre_usuario")]
    public string? NombreUsuario { get; set; }

    /// <summary>
    /// Email del usuario (viene directamente del backend)
    /// </summary>
    [JsonPropertyName("email_usuario")]
    public string? EmailUsuario { get; set; }

    /// <summary>
    /// Nombre a mostrar en UI con fallback a "Usuario #id"
    /// </summary>
    [JsonIgnore]
    public string NombreMostrar =>
        !string.IsNullOrWhiteSpace(NombreUsuario)
            ? NombreUsuario
            : $"Usuario #{IdUsuario}";

    /// <summary>
    /// Email a mostrar en UI, vacío si no está disponible
    /// </summary>
    [JsonIgnore]
    public string EmailMostrar => EmailUsuario ?? "";

    /// <summary>
    /// Indica si hay email disponible para mostrar
    /// </summary>
    [JsonIgnore]
    public bool TieneEmail => !string.IsNullOrWhiteSpace(EmailUsuario);

    [JsonIgnore]
    public string RolProyectoDisplay => RolHelper.ToSpanish(RolProyecto);
}

public class ProyectoUsuarioCreateRequest
{
    [JsonPropertyName("id_usuario")]
    public int IdUsuario { get; set; }

    [JsonPropertyName("rol_proyecto")]
    public string RolProyecto { get; set; } = "viewer";
}

public class ProyectoUsuarioUpdateRequest
{
    [JsonPropertyName("rol_proyecto")]
    public string RolProyecto { get; set; } = "viewer";
}

public class ProyectoUsuarioEmailCreateRequest
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("rol_proyecto")]
    public string RolProyecto { get; set; } = "viewer";
}

public static class RolHelper
{
    private static readonly Dictionary<string, string> _toSpanish = new()
    {
        { "owner", "Propietario" },
        { "editor", "Editor" },
        { "viewer", "Lector" }
    };

    private static readonly Dictionary<string, string> _toEnglish = new()
    {
        { "Propietario", "owner" },
        { "Editor", "editor" },
        { "Lector", "viewer" }
    };

    public static List<string> RolesEnEspanol { get; } = new() { "Propietario", "Editor", "Lector" };

    public static string ToSpanish(string rolBackend) =>
        _toSpanish.TryGetValue(rolBackend, out var spanish) ? spanish : rolBackend;

    public static string ToEnglish(string rolDisplay) =>
        _toEnglish.TryGetValue(rolDisplay, out var english) ? english : rolDisplay;
}
