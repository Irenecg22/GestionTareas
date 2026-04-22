namespace GestionTareas.Services;

public interface IRestService<T>
{
    Task<List<T>> GetAllAsync();
    Task<bool> DeleteAsync(int id);
    
}

