namespace GestionTareas.Model;

/// <summary>
/// Modelo para el selector de asignación de tareas
/// </summary>
public class MiembroAsignacion
{
    public int? IdUsuario { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string? EmailUsuario { get; set; }
    public string? RolProyecto { get; set; }

    public bool EsSinAsignar => IdUsuario == null;

    public string DisplayText => EsSinAsignar ? "Sin asignar" : NombreUsuario;

    /// <summary>
    /// Crea la opción "Sin asignar"
    /// </summary>
    public static MiembroAsignacion SinAsignar() => new()
    {
        IdUsuario = null,
        NombreUsuario = "Sin asignar"
    };

    /// <summary>
    /// Crea un miembro desde ProyectoUsuario
    /// </summary>
    public static MiembroAsignacion FromProyectoUsuario(ProyectoUsuario miembro) => new()
    {
        IdUsuario = miembro.IdUsuario,
        NombreUsuario = miembro.NombreMostrar,
        EmailUsuario = miembro.EmailUsuario,
        RolProyecto = miembro.RolProyectoDisplay
    };
}
