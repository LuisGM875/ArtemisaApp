using ArtemisaApp.ViewModel;

namespace ArtemisaApp.View;

public partial class EditUser : ContentPage
{
	public EditUser()
	{
		InitializeComponent();
        BindingContext = new EditUserViewModel();
    }
}