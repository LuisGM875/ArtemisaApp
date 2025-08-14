using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class LoginViewModel : INotifyPropertyChanged
{
    private string username;
    private string password;
    private string message;

    public string Username
    {
        get => username;
        set { username = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => password;
        set { password = value; OnPropertyChanged(); }
    }

    public string Message
    {
        get => message;
        set { message = value; OnPropertyChanged(); }
    }

    public ICommand LoginCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new Command(OnLogin);
    }

    private void OnLogin()
    {
        // Lógica de autenticación simple (reemplaza por tu lógica real)
        if (Username == "admin" && Password == "1234")
            Message = "¡Login exitoso!";
        else
            Message = "Usuario o contraseña incorrectos.";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}           