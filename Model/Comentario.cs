namespace GestionTareas.Model;

public class Comentario
{
    public int Id { get; set; }
    public string Texto { get; set; }
    public DateTime Fecha { get; set; }

    public int TareaId { get; set; }
    public Tarea Tarea { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; }
}
