using System.ComponentModel;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Windows.Input;
using System.Collections.ObjectModel;
using ArtemisaApp.Models;
using ArtemisaApp.View;
using CommunityToolkit.Mvvm.Input;


namespace ArtemisaApp.ViewModel
{
    public class PrincipalViewModel : BindableObject, IQueryAttributable
    {
        private string _token;
        private string _fullName;
        private double _wallet;
        private ObservableCollection<TransactionHistoryItem> _transactions = new();
        private string _photoPath;
        private bool _isMenuVisible;

        public PrincipalViewModel(string token)
        {
            Token = token;
            TransferCommand = new Command(async () => await GoToTransferPage());
            EditUserCommand = new RelayCommand(OnEditUser);
            LogoutCommand = new Command(async () => await LogoutAsync());
            ShowMenuCommand = new Command(() => IsMenuVisible = !IsMenuVisible);
        }

        public PrincipalViewModel()
        {
            TransferCommand = new Command(async () => await GoToTransferPage());
            EditUserCommand = new RelayCommand(OnEditUser);
            LogoutCommand = new Command(async () => await LogoutAsync());
            ShowMenuCommand = new Command(() => IsMenuVisible = !IsMenuVisible);
        }

        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                OnPropertyChanged();
            }
        }

        public string FullName
        {
            get => _fullName;
            set
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }

        public double Wallet
        {
            get => _wallet;
            set
            {
                _wallet = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<TransactionHistoryItem> Transactions
        {
            get => _transactions;
            set
            {
                _transactions = value;
                OnPropertyChanged();
            }
        }

        public string PhotoPath
        {
            get => _photoPath;
            set
            {
                _photoPath = value;
                OnPropertyChanged();
            }
        }

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set
            {
                _isMenuVisible = value;
                OnPropertyChanged();
            }
        }

        public ICommand TransferCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand ShowMenuCommand { get; }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("token", out var tokenObj) && tokenObj is string token)
            {
                await FetchProfileAsync(token);
                await FetchTransactionHistoryAsync(token);
            }
        }

        private async Task FetchProfileAsync(string token)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://wsartemisaapi.onrender.com/api/v1/profile");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var firstName = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null;
                    var lastName = root.TryGetProperty("lastName", out var lastNameProp) ? lastNameProp.GetString() : null;
                    FullName = $"{firstName} {lastName}".Trim();

                    Wallet = root.TryGetProperty("wallet", out var walletProp) ? walletProp.GetDouble() : 0.0;
                    PhotoPath = root.TryGetProperty("photoPath", out var photoProp) ? photoProp.GetString() : null;
                }
                else
                {
                    FullName = "User";
                    Wallet = 0.0;
                }
            }
            catch
            {
                FullName = "User";
                Wallet = 0.0;
            }
        }

        private async Task FetchTransactionHistoryAsync(string token)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://wsartemisaapi.onrender.com/api/v1/transaction/history");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<TransactionHistoryItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Transactions = new ObservableCollection<TransactionHistoryItem>(items ?? new List<TransactionHistoryItem>());
                }
            }
            catch
            {
                // Optional error handling
            }
        }

        private async Task GoToTransferPage()
        {
            if (!string.IsNullOrEmpty(Token))
            {
                await Shell.Current.GoToAsync($"Transfer?token={Token}");
            }
        }

        private async void OnEditUser()
        {
            if (!string.IsNullOrEmpty(Token))
            {
                await Shell.Current.GoToAsync($"{nameof(EditUser)}?token={Token}");
            }
        }

        private async Task LogoutAsync()
        {
            // Limpia datos sensibles si es necesario
            Token = null;
            FullName = null;
            Wallet = 0;
            PhotoPath = null;
            Transactions.Clear();

            // Navega a la pantalla de login y limpia la pila de navegación
            await Shell.Current.GoToAsync("///Login");
        }
    }
}