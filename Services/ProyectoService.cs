using GestionTareas.Model;
namespace GestionTareas.Services;

public class ProyectoService : IRestService<Proyecto>
{
    public async Task<List<Proyecto>> GetAllAsync()
        {
            await Task.Delay(500);

            return new List<Proyecto>
        {
            new Proyecto
            {
                Id = 1,
                Nombre = "App TFG",
                Descripcion = "Aplicación de gestión de tareas",
                Tareas = new List<Tarea>
        {
            new Tarea { UsuarioAsignado = new Usuario { Nombre = "Juan" } },
            new Tarea { UsuarioAsignado = new Usuario { Nombre = "Marta" } }
        }
            },
            new Proyecto
            {
                Id = 2,
                Nombre = "Web empresa",
                Descripcion = "Página corporativa",
                Tareas = new List<Tarea>
        {
            new Tarea { UsuarioAsignado = new Usuario { Nombre = "Juan" } },
            new Tarea { UsuarioAsignado = new Usuario { Nombre = "Marta" } }
        }
            }
        };
        }
    }

