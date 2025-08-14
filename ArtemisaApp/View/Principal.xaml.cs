using ArtemisaApp.ViewModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Xaml;
using System.Collections.Generic;

namespace ArtemisaApp.View
{
    public partial class Principal : ContentPage, IQueryAttributable
    {
        public Principal()
        {
            InitializeComponent();
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("token", out var tokenIdObj) && tokenIdObj is string token)
            {
                BindingContext = new PrincipalViewModel(token);
            }
        }

        private async void OnProfileTapped(object sender, EventArgs e)
        {
            string action = await Application.Current.MainPage.DisplayActionSheet(
                "Opciones de usuario", "Cancelar", null, "Editar usuario", "Cerrar sesión");

            if (action == "Editar usuario")
            {
                // Ejecuta el comando de editar usuario
                if (BindingContext is PrincipalViewModel vm && vm.EditUserCommand.CanExecute(null))
                    vm.EditUserCommand.Execute(null);
            }
            else if (action == "Cerrar sesión")
            {
                // Ejecuta el comando de cerrar sesión
                if (BindingContext is PrincipalViewModel vm && vm.LogoutCommand.CanExecute(null))
                    vm.LogoutCommand.Execute(null);
            }
        }
    }
}