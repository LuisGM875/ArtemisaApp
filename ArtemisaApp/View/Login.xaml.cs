using ArtemisaApp.ViewModel;

namespace ArtemisaApp.View;


public partial class Login : ContentPage
{
    public Login()
    {
        InitializeComponent();
        BindingContext = new LoginViewModel();
    }
}