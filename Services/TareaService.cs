using GestionTareas.Model;

namespace GestionTareas.Services;

public class TareaService : IRestService<Tarea>
{
    public async Task<List<Tarea>> GetAllAsync()
    {
        await Task.Delay(500); 

        return new List<Tarea>
            {
                new Tarea
                {
                    Id = 1,
                    Titulo = "Diseñar login",
                    Descripcion = "Pantalla de inicio de sesión",
                    Estado = EstadoTarea.Pendiente,
                    Prioridad = PrioridadTarea.Alta,
                    UsuarioAsignado = new Usuario { Nombre = "Admin" }
                },
                new Tarea
                {
                    Id = 2,
                    Titulo = "Crear base de datos",
                    Descripcion = "Modelo relacional",
                    Estado = EstadoTarea.EnProgreso,
                    Prioridad = PrioridadTarea.Media,
                    UsuarioAsignado = new Usuario { Nombre = "Admin" }
                },
                new Tarea
                {
                    Id = 3,
                    Titulo = "Pantalla de tareas",
                    Descripcion = "Lista de tareas",
                    Estado = EstadoTarea.Bloqueada,
                    Prioridad = PrioridadTarea.Alta,
                    UsuarioAsignado = new Usuario { Nombre = "Sin asignar" }
                }
            };
    }
}
