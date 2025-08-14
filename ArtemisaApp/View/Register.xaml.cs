using ArtemisaApp.ViewModel;

namespace ArtemisaApp.View;

public partial class Register : ContentPage
{
	public Register()
	{
		InitializeComponent();
        BindingContext = new RegisterViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RegisterViewModel vm)
        {
            await vm.LoadCardBrandsAsync();
        }
    }
}