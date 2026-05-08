using GestionTareas.View;
using Microsoft.Extensions.DependencyInjection;
namespace GestionTareas
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var loginView = _serviceProvider.GetRequiredService<LoginView>();
            return new Window(new NavigationPage(loginView));
        }
    }
}