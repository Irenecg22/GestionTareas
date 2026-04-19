using GestionTareas.Model;
using GestionTareas.Services;
using GestionTareas.View;
using GestionTareas.ViewModel;
using Microsoft.Extensions.Logging;

namespace GestionTareas
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()        // inicialización correcta de LiveCharts
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            builder.Services.AddTransient<IRestService<Tarea>,TareaService>();
            builder.Services.AddTransient<IRestService<Proyecto>, ProyectoService>();
            builder.Services.AddTransient<IRestService<Usuario>, UserService>();

            builder.Services.AddTransient<PanelPrincipalViewModel>();
            builder.Services.AddTransient<ProyectoViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            //builder.Services.AddTransient<ProyectoDetalleViewModel>();
            builder.Services.AddTransient<TareaViewModel>();
            builder.Services.AddTransient<TareaDetalleViewModel>();

            builder.Services.AddTransient<TareaView>();
            builder.Services.AddTransient<TareaDetalleView>();
            builder.Services.AddTransient<ProyectoView>();
            builder.Services.AddTransient<SettingsView>();
            //builder.Services.AddTransient<ProyectoDetalleView>();
            builder.Services.AddTransient<PanelPrincipal>();

            return builder.Build();
        }
    }
}

