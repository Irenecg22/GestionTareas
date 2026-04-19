using System.Text.Json.Serialization;

namespace GestionTareas.Model;

public class Rol
{
    [JsonPropertyName("id_rol")]
    public int Id { get; set; }

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }

    public List<Usuario> Usuarios { get; set; }
}