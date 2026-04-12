namespace GestionTareas.Model;

public class Tarea
{
    public int Id { get; set; }
    public string Titulo { get; set; }
    public string Descripcion { get; set; }

    public EstadoTarea Estado { get; set; }
    public PrioridadTarea Prioridad { get; set; }

    public int ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; }

    public int? UsuarioId { get; set; }
    public Usuario UsuarioAsignado { get; set; }

    public List<Comentario> Comentarios { get; set; }
}

public enum EstadoTarea
{
    Pendiente,
    EnProgreso,
    Bloqueada,
    Finalizada
}

public enum PrioridadTarea
{
    Baja,
    Media,
    Alta
}
