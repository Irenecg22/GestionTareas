using CommunityToolkit.Mvvm.ComponentModel;
using GestionTareas.Model;

namespace GestionTareas.ViewModel;

[QueryProperty(nameof(Proyecto), "Proyecto")]
public partial class ProyectoDetalleViewModel : ObservableObject
{
    [ObservableProperty]
    private Proyecto proyecto;
}
