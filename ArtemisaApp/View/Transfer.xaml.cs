using ArtemisaApp.ViewModel;
using Microsoft.Maui.Controls;
using System;

namespace ArtemisaApp.View
{
    public partial class Transfer : ContentPage
    {
        public Transfer()
        {
            InitializeComponent();
            BindingContext = new TransferViewModel();
        }

        private async void Regresar(object sender, EventArgs e)
        {
            if (BindingContext is TransferViewModel vm && !string.IsNullOrEmpty(vm.Token))
            {
                await Shell.Current.GoToAsync($"Principal?token={vm.Token}");
            }
            else
            {
                await Shell.Current.GoToAsync("Principal");
            }
        }
    }
}