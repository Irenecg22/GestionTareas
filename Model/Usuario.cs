using System.Text.Json.Serialization;

namespace GestionTareas.Model;

public class Usuario
{
    [JsonPropertyName("id_usuario")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("password")]
    public string Password { get; set; }

    [JsonPropertyName("id_rol")]
    public int RolId { get; set; }

    public Rol Rol { get; set; }

    public List<Tarea> TareasAsignadas { get; set; }
}