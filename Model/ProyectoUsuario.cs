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
    [JsonPropertyName("nombre_usuario")]
    public string? NombreUsuario { get; set; }
    [JsonPropertyName("email_usuario")]
    public string? EmailUsuario { get; set; }
    [JsonIgnore]
    public string NombreMostrar =>
        !string.IsNullOrWhiteSpace(NombreUsuario)
            ? NombreUsuario
            : $"Usuario #{IdUsuario}";
    [JsonIgnore]
    public string EmailMostrar => EmailUsuario ?? "";
    [JsonIgnore]
    public bool TieneEmail => !string.IsNullOrWhiteSpace(EmailUsuario);
    [JsonIgnore]
    public string RolProyectoDisplay => RolHelper.ToSpanish(RolProyecto);

    [JsonIgnore]
    public string Iniciales
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(NombreUsuario))
            {
                var palabras = NombreUsuario.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (palabras.Length >= 2)
                    return $"{palabras[0][0]}{palabras[1][0]}".ToUpper();
                if (palabras.Length == 1 && palabras[0].Length >= 2)
                    return palabras[0].Substring(0, 2).ToUpper();
                if (palabras.Length == 1 && palabras[0].Length == 1)
                    return palabras[0].ToUpper();
            }
            if (!string.IsNullOrWhiteSpace(EmailUsuario))
            {
                var partes = EmailUsuario.Split('@');
                if (partes.Length > 0 && partes[0].Length >= 2)
                    return partes[0].Substring(0, 2).ToUpper();
            }
            return "??";
        }
    }

    [JsonIgnore]
    public bool EsPropietario => RolProyecto == "owner";
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
