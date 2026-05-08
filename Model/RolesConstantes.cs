namespace GestionTareas.Model;
public static class RolesConstantes
{
    public const int ADMIN = 1;
    public const int USUARIO = 2;
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
