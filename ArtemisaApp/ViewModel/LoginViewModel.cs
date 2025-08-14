using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using ArtemisaApp.Models;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace ArtemisaApp.ViewModel
{
    public class LoginViewModel : BindableObject
    {
        private User _user = new();
        private bool _isLoading;
        private bool _isPasswordHidden = true;

        public User User
        {
            get => _user;
            set { _user = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool IsPasswordHidden
        {
            get => _isPasswordHidden;
            set { _isPasswordHidden = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand GoToRegisterCommand { get; }
        public ICommand TogglePasswordVisibilityCommand { get; }
        public ICommand FingerprintLoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new Command(async () => await LoginAsync());
            GoToRegisterCommand = new Command(async () => await GoToRegisterAsync());
            TogglePasswordVisibilityCommand = new Command(() => IsPasswordHidden = !IsPasswordHidden);
            FingerprintLoginCommand = new Command(async () => await LoginWithFingerprintAsync());
        }

        private async Task GoToRegisterAsync()
        {
            await Shell.Current.GoToAsync("Register");
        }

        private async Task LoginAsync()
        {
            IsLoading = true;
            try
            {
                var client = new HttpClient();
                var loginData = new
                {
                    email = User.email,
                    password = User.password
                };

                var json = JsonSerializer.Serialize(loginData);
                System.Diagnostics.Debug.WriteLine($"JSON enviado: {json}");

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://wsartemisaapi.onrender.com/api/v1/auth/login", content);

                var responseBody = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Respuesta completa: {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonDoc = JsonDocument.Parse(responseBody);
                    var token = jsonDoc.RootElement.TryGetProperty("token", out var t) ? t.GetString() : null;

                    if (!string.IsNullOrEmpty(token))
                    {
                        System.Diagnostics.Debug.WriteLine($"Token recibido: {token}");
                        await SecureStorage.SetAsync("auth_token", token);
                        await SecureStorage.SetAsync("user_email", User.email);
                        await SecureStorage.SetAsync("user_password", User.password);

                        // Al hacer login exitoso, guarda la cuenta en una lista
                        var cuentasJson = await SecureStorage.GetAsync("cuentas");
                        var cuentas = string.IsNullOrEmpty(cuentasJson) ? new List<User>() : JsonSerializer.Deserialize<List<User>>(cuentasJson);
                        if (!cuentas.Any(u => u.email == User.email))
                            cuentas.Add(new User { email = User.email, password = User.password });
                        await SecureStorage.SetAsync("cuentas", JsonSerializer.Serialize(cuentas));

                        if (Shell.Current != null)
                        {
                            await Shell.Current.GoToAsync($"Principal?token={token}");
                        }
                        else if (Application.Current?.MainPage != null)
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Navigation system is not initialized.", "OK");
                        }
                    }
                    else if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Token o ID de usuario no recibido.", "OK");
                    }
                }
                else if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Usuario o contraseña incorrectos", "OK");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoginWithFingerprintAsync()
        {
            var result = await CrossFingerprint.Current.AuthenticateAsync(new AuthenticationRequestConfiguration(
                "Autenticación biométrica",
                "Usa tu huella digital para iniciar sesión"));

            if (result.Authenticated)
            {
                // Recupera las credenciales almacenadas de forma segura
                var email = await SecureStorage.GetAsync("user_email");
                var password = await SecureStorage.GetAsync("user_password");

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
                {
                    User.email = email;
                    User.password = password;
                    await LoginAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No hay credenciales guardadas. Inicia sesión manualmente primero.", "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No se pudo autenticar", "OK");
            }
        }
    }
}
