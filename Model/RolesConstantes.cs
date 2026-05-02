namespace GestionTareas.Model;

/// <summary>
/// Constantes para los IDs de roles de usuario en el sistema.
/// </summary>
public static class RolesConstantes
{
    /// <summary>
    /// Rol de Administrador - Tiene permisos completos en el sistema.
    /// </summary>
    public const int ADMIN = 1;

    /// <summary>
    /// Rol de Usuario Normal - Permisos estándar para gestión de tareas.
    /// </summary>
    public const int USUARIO = 2;

    /// <summary>
    /// Obtiene el nombre del rol según su ID.
    /// </summary>
    public static string ObtenerNombreRol(int rolId)
    {
        return rolId switch
        {
            ADMIN => "Administrador",
            USUARIO => "Usuario",
            _ => "Desconocido"
        };
    }
}
