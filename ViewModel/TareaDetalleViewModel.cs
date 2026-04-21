using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;

namespace GestionTareas.ViewModel
{
    [QueryProperty(nameof(Tarea), "Tarea")]
    public partial class TareaDetalleViewModel : ObservableObject
    {
        [ObservableProperty]
        private Tarea tarea;
    }
}
