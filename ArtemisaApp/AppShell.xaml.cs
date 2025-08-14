using ArtemisaApp.View;

namespace ArtemisaApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute("Principal", typeof(Principal));
            Routing.RegisterRoute("Register", typeof(ArtemisaApp.View.Register));
            Routing.RegisterRoute("Transfer", typeof(ArtemisaApp.View.Transfer));
            Routing.RegisterRoute(nameof(EditUser), typeof(EditUser));
            Routing.RegisterRoute("Login", typeof(ArtemisaApp.View.Login));
        }
    }
}
