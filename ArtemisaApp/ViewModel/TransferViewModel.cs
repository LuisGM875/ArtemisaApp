using ArtemisaApp.Models;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ArtemisaApp.ViewModel
{
    public class TransferViewModel : BindableObject, IQueryAttributable
    {
        private string _token;
        private double _wallet;
        private string _selectedPerson;
        private ObservableCollection<UserItem> _users = new();
        private UserItem _selectedUser;
        private TransferItem _transfer = new();

        public string Token
        {
            get => _token;
            set { _token = value; OnPropertyChanged(); }
        }

        public double Wallet
        {
            get => _wallet;
            set { _wallet = value; OnPropertyChanged(); }
        }

        public string SelectedPerson
        {
            get => _selectedPerson;
            set { _selectedPerson = value; OnPropertyChanged(); }
        }

        public ObservableCollection<UserItem> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public UserItem SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                SelectedPerson = value?.FullName;
                OnPropertyChanged();
            }
        }

        public TransferItem Transfer
        {
            get => _transfer;
            set { _transfer = value; OnPropertyChanged(); }
        }

        public ICommand TransferCommand { get; }

        public TransferViewModel()
        {
            TransferCommand = new Command(async () => await TransferAsync());
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("token", out var tokenObj) && tokenObj is string token)
            {
                Token = token;
                Transfer.FromUserId = GetUserIdFromToken(token);
                await FetchProfileAsync(token);
                await FetchUsersAsync(token);
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

                    if (root.TryGetProperty("user", out var userProp))
                    {
                        Wallet = userProp.TryGetProperty("wallet", out var walletProp) ? walletProp.GetDouble() : 0.0;
                    }
                    else
                    {
                        Wallet = root.TryGetProperty("wallet", out var walletProp) ? walletProp.GetDouble() : 0.0;
                    }
                }
                else
                {
                    Wallet = 0.0;
                }
            }
            catch
            {
                Wallet = 0.0;
            }
        }

        private async Task FetchUsersAsync(string token)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync("https://wsartemisaapi.onrender.com/api/v1/user");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var items = JsonSerializer.Deserialize<List<UserItem>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    Users = new ObservableCollection<UserItem>(items ?? new List<UserItem>());
                }
            }
            catch
            {

            }
        }

        private async Task TransferAsync()
        {
            // Usar cultura invariante para evitar problemas con el punto/coma decimal
            if (!double.TryParse(Transfer.Amount.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double amountToTransfer) || amountToTransfer <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Ingresa un monto válido.", "OK");
                return;
            }

            if (amountToTransfer > Wallet)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No puedes transferir más de tu saldo disponible.", "OK");
                return;
            }

            if (SelectedUser == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Selecciona un destinatario.", "OK");
                return;
            }

            var transaction = new
            {
                FromUserId = Transfer.FromUserId,
                ToUserId = SelectedUser.Id,
                Amount = amountToTransfer,
                OperationType = Transfer.OperationType,
                Status = Transfer.Status
            };

            System.Diagnostics.Debug.WriteLine("Datos enviados a /api/v1/transaction:");
            System.Diagnostics.Debug.WriteLine($"FromUserId: {transaction.FromUserId}");
            System.Diagnostics.Debug.WriteLine($"ToUserId: {transaction.ToUserId}");
            System.Diagnostics.Debug.WriteLine($"Amount: {transaction.Amount}");
            System.Diagnostics.Debug.WriteLine($"OperationType: {transaction.OperationType}");
            System.Diagnostics.Debug.WriteLine($"Status: {transaction.Status}");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token);

                var json = JsonSerializer.Serialize(transaction);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://wsartemisaapi.onrender.com/api/v1/transaction", content);

                if (response.IsSuccessStatusCode)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Transferencia realizada correctamente.", "OK");
                    await Shell.Current.GoToAsync($"///Principal?token={Token}");
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"No se pudo realizar la transferencia: {error}", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Ocurrió un error: {ex.Message}", "OK");
            }
        }

        private string GetUserIdFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                return string.Empty;

            try
            {
                var parts = token.Split('.');
                if (parts.Length < 2)
                    return string.Empty;

                var payload = parts[1];
                payload = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var bytes = Convert.FromBase64String(payload);
                var json = Encoding.UTF8.GetString(bytes);

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                return root.TryGetProperty("nameid", out var idProp) ? idProp.GetString() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}