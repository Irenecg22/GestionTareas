using System.Text.Json.Serialization;
namespace GestionTareas.Model;
public class Proyecto
{
    [JsonPropertyName("id_proyecto")]
    public int Id { get; set; }
    [JsonPropertyName("nombre")]
    public string Nombre { get; set; }
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
    [JsonPropertyName("fecha_creacion")]
    public string? FechaCreacion { get; set; }
    [JsonPropertyName("creado_por_id")]
    public int CreadoPorId { get; set; }
    [JsonIgnore]
    public List<Tarea> Tareas { get; set; } = new();
}