using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;
using CommunityToolkit.Mvvm.Input;
using GestionTareas.Services;
using System.Collections.ObjectModel;
namespace GestionTareas.ViewModel;
[QueryProperty(nameof(Proyecto), "Proyecto")]
public partial class ProyectoDetalleViewModel : ObservableObject
{
    private readonly TareaService _tareaService;
    private readonly ProyectoService _proyectoService;
    [ObservableProperty]
    private Proyecto proyecto;
    [ObservableProperty]
    private ObservableCollection<Tarea> tareasFiltradas;
    [ObservableProperty]
    private ObservableCollection<ProyectoUsuario> miembros;
    [ObservableProperty]
    private string inputEmail = string.Empty;
    [ObservableProperty]
    private string inputRolDisplay = "Lector";
    public List<string> RolesProyectoDisplay { get; } = RolHelper.RolesEnEspanol;
    public ProyectoDetalleViewModel(TareaService tareaService, ProyectoService proyectoService)
    {
        _tareaService = tareaService;
        _proyectoService = proyectoService;
        TareasFiltradas = new ObservableCollection<Tarea>();
        Miembros = new ObservableCollection<ProyectoUsuario>();
    }
    partial void OnProyectoChanged(Proyecto value)
    {
        if (value != null)
        {
            _ = CargarDatosDelProyecto(value.Id);
        }
    }
    private async Task CargarDatosDelProyecto(int proyectoId)
    {
        await CargarTareasDelProyecto(proyectoId);
        await CargarMiembrosDelProyecto(proyectoId);
    }
    private async Task CargarTareasDelProyecto(int proyectoId)
    {
        var todasLasTareas = await _tareaService.GetAllAsync();
        var tareasRelacionadas = todasLasTareas.Where(t => t.ProyectoId == proyectoId).ToList();
        TareasFiltradas.Clear();
        foreach (var tarea in tareasRelacionadas)
        {
            TareasFiltradas.Add(tarea);
        }
    }
    private async Task CargarMiembrosDelProyecto(int proyectoId)
    {
        var listaMiembros = await _proyectoService.GetMiembrosAsync(proyectoId);
        Miembros.Clear();
        foreach (var miembro in listaMiembros)
        {
            Miembros.Add(miembro);
        }
    }
    [RelayCommand]
    private async Task AddMiembro()
    {
        if (Proyecto == null) return;
        if (string.IsNullOrWhiteSpace(InputEmail) || !InputEmail.Contains('@'))
        {
            await Shell.Current.DisplayAlert("Error", "Introduce un email válido.", "OK");
            return;
        }
        if (string.IsNullOrWhiteSpace(InputRolDisplay))
        {
            await Shell.Current.DisplayAlert("Error", "Selecciona un rol para el miembro.", "OK");
            return;
        }
        var rolBackend = RolHelper.ToEnglish(InputRolDisplay);
        var (success, statusCode) = await _proyectoService.AddMiembroPorEmailAsync(Proyecto.Id, InputEmail.Trim(), rolBackend);
        if (success)
        {
            InputEmail = string.Empty;
            InputRolDisplay = "Lector";
            await CargarMiembrosDelProyecto(Proyecto.Id);
        }
        else if (statusCode == 403)
        {
            await Shell.Current.DisplayAlert("Sin permisos",
                "No tienes permisos para añadir miembros.", "OK");
        }
        else if (statusCode == 404)
        {
            await Shell.Current.DisplayAlert("No encontrado",
                "No existe ningún usuario con ese email.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo añadir el miembro.", "OK");
        }
    }
    [RelayCommand]
    private async Task UpdateRolMiembro(ProyectoUsuario miembro)
    {
        if (Proyecto == null || miembro == null) return;
        string nuevoRolDisplay = await Shell.Current.DisplayActionSheet(
        $"Cambiar rol de {miembro.NombreMostrar}",
        "Cancelar", null,
        "Propietario", "Editor", "Lector");
        if (nuevoRolDisplay == "Cancelar" || string.IsNullOrEmpty(nuevoRolDisplay))
            return;
        var nuevoRol = RolHelper.ToEnglish(nuevoRolDisplay);
        if (nuevoRol == miembro.RolProyecto) return;
        var (success, statusCode) = await _proyectoService.UpdateMiembroRolAsync(Proyecto.Id, miembro.IdUsuario, nuevoRol);
        if (success)
        {
            await CargarMiembrosDelProyecto(Proyecto.Id);
        }
        else if (statusCode == 403)
        {
            await Shell.Current.DisplayAlert("Sin permisos",
                "No tienes permisos para gestionar miembros de este proyecto.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo cambiar el rol.", "OK");
        }
    }
    [RelayCommand]
    private async Task RemoveMiembro(ProyectoUsuario miembro)
    {
        if (Proyecto == null || miembro == null) return;
        bool confirmar = await Shell.Current.DisplayAlert(
         "Eliminar miembro",
         $"¿Seguro que quieres eliminar a {miembro.NombreMostrar} del proyecto?",
         "Sí, eliminar", "Cancelar");
        if (!confirmar) return;
        var (success, statusCode) = await _proyectoService.RemoveMiembroAsync(Proyecto.Id, miembro.IdUsuario);
        if (success)
        {
            await CargarMiembrosDelProyecto(Proyecto.Id);
        }
        else if (statusCode == 403)
        {
            await Shell.Current.DisplayAlert("Sin permisos",
                "No tienes permisos para gestionar miembros de este proyecto.", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Error", "No se pudo eliminar al miembro.", "OK");
        }
    }
}