using System.Text.Json.Serialization;

namespace GestionTareas.Model;

public class Tarea
{
    [JsonPropertyName("id_tarea")]
    public int Id { get; set; }

    [JsonPropertyName("titulo")]
    public string Titulo { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; }   

    [JsonPropertyName("id_proyecto")]
    public int ProyectoId { get; set; }

    [JsonPropertyName("id_usuario_asignado")]
    public int? UsuarioId { get; set; }
}