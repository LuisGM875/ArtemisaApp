using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using ArtemisaApp.Models;

namespace ArtemisaApp.ViewModel
{
    public class RegisterViewModel : BindableObject
    {
        private RegisterItem _register = new();
        private ObservableCollection<Branch> _cardBrands = new();
        private Branch _selectedCardBrand;
        private bool _isLoading;

        public RegisterItem Register
        {
            get => _register;
            set { _register = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Branch> CardBrands
        {
            get => _cardBrands;
            set { _cardBrands = value; OnPropertyChanged(); }
        }

        public Branch SelectedCardBrand
        {
            get => _selectedCardBrand;
            set
            {
                _selectedCardBrand = value;
                Register.CardBrandId = value?.Id;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand GoToLoginCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new Command(async () => await RegisterAsync());
            GoToLoginCommand = new Command(async () => await GoToLoginAsync());
        }

        public async Task LoadCardBrandsAsync()
        {
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync("https://wsartemisaapi.onrender.com/api/v1/cardbrand");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var brands = JsonSerializer.Deserialize<List<Branch>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    CardBrands = new ObservableCollection<Branch>(brands ?? new List<Branch>());
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error al cargar marcas: " + ex.Message);
            }
        }

        private async Task GoToLoginAsync()
        {
            await Shell.Current.GoToAsync("//Login");
        }

        private async Task RegisterAsync()
        {
            if (Register.Password != Register.ConfirmPassword)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Las contraseñas no coinciden", "OK");
                return;
            }
            IsLoading = true;
            try
            {
                var client = new HttpClient();
                var walletValue = Register.Wallet == 0 ? 0 : Register.Wallet;

                var registerData = new
                {
                    email = Register.Email,
                    password = Register.Password,
                    name = Register.Name,
                    lastName = Register.LastName,
                    cardBrandId = Register.CardBrandId,
                    wallet = walletValue
                };

                var json = JsonSerializer.Serialize(registerData);
                System.Diagnostics.Debug.WriteLine("JSON enviado: " + json);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://wsartemisaapi.onrender.com/api/v1/auth/register", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
                    await GoToLoginAsync();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo registrar: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}