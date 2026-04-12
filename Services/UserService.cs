using GestionTareas.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace GestionTareas.Services
{
    public class UserService : IRestService<Usuario>
    {
        public async Task<List<Usuario>> GetAllAsync()
        {
            await Task.Delay(100); 
            return new List<Usuario>
        {
            new Usuario
            {
                Nombre = "Alex Morgan",
                Email = "alex.morgan@company.com"
            }
        };
        }
    }
}
