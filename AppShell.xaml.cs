using GestionTareas.View;

namespace GestionTareas
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            RegisterRoutes();
        }

        private void RegisterRoutes()
        {
            Routing.RegisterRoute("tareaDetalle", typeof(TareaDetalleView));
            Routing.RegisterRoute("proyectoDetalle", typeof(ProyectoDetalleView));
            Routing.RegisterRoute(nameof(CrearProyectoView), typeof(CrearProyectoView));
            Routing.RegisterRoute(nameof(CrearTareaView), typeof(CrearTareaView));
        }
    }
}