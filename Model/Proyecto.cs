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
    [JsonIgnore]
    public List<ProyectoUsuario> Miembros { get; set; } = new();

    // Propiedades para el preview de miembros en la tarjeta
    [JsonIgnore]
    public List<ProyectoUsuario> MiembrosVisibles => Miembros.Take(3).ToList();

    [JsonIgnore]
    public int MiembrosRestantes => Math.Max(0, Miembros.Count - 3);

    [JsonIgnore]
    public bool TieneMasDeTresMiembros => Miembros.Count > 3;

    [JsonIgnore]
    public bool TieneMiembros => Miembros.Any();

    [JsonIgnore]
    public bool NoTieneMiembros => !Miembros.Any();
}
