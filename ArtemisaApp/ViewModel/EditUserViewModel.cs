using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls; // Para IQueryAttributable

namespace ArtemisaApp.ViewModel
{
    public class EditUserViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private string _name;
        private string _lastName;
        private string _photoPath;
        private string _token;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string LastName
        {
            get => _lastName;
            set { _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }

        public string PhotoPath
        {
            get => _photoPath;
            set { _photoPath = value; OnPropertyChanged(nameof(PhotoPath)); }
        }

        public ICommand TakePhotoCommand { get; }
        public ICommand PickPhotoCommand { get; }
        public ICommand SaveCommand { get; }

        public EditUserViewModel()
        {
            TakePhotoCommand = new AsyncRelayCommand(TakePhotoAsync);
            PickPhotoCommand = new AsyncRelayCommand(PickPhotoAsync);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        private async Task TakePhotoAsync()
        {
            try
            {
                if (await Permissions.CheckStatusAsync<Permissions.Camera>() != PermissionStatus.Granted)
                {
                    await Permissions.RequestAsync<Permissions.Camera>();
                }

                var photo = await MediaPicker.Default.CapturePhotoAsync();
                if (photo != null)
                {
                    System.Diagnostics.Debug.WriteLine("camara abierta");
                    var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using var stream = await photo.OpenReadAsync();
                    using var newStream = File.OpenWrite(newFile);
                    await stream.CopyToAsync(newStream);
                    PhotoPath = newFile;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("camara cerrada");
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("camara cerrada");
                // Maneja errores aquí si lo deseas
            }
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.Default.PickPhotoAsync();
                if (photo != null)
                {
                    System.Diagnostics.Debug.WriteLine("imagen seleccionada");
                    var newFile = Path.Combine(FileSystem.CacheDirectory, photo.FileName);
                    using var stream = await photo.OpenReadAsync();
                    using var newStream = File.OpenWrite(newFile);
                    await stream.CopyToAsync(newStream);
                    PhotoPath = newFile;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("selección cancelada");
                }
            }
            catch (Exception)
            {
                System.Diagnostics.Debug.WriteLine("selección cancelada");
                // Maneja errores aquí si lo deseas
            }
        }

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("token", out var tokenObj) && tokenObj is string token)
            {
                _token = token;
                await FetchProfileAsync(token);
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

                    Name = root.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : "";
                    LastName = root.TryGetProperty("lastName", out var lastNameProp) ? lastNameProp.GetString() : "";
                    PhotoPath = root.TryGetProperty("photoPath", out var photoProp) ? photoProp.GetString() : "";
                }
            }
            catch
            {
                // Manejo de errores opcional
            }
        }

        private async Task SaveAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

                var data = new
                {
                    name = Name,
                    lastName = LastName,
                    photoPath = PhotoPath // Si el backend espera la ruta o URL de la foto
                };

                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync("https://wsartemisaapi.onrender.com/api/v1/profile", content);

                if (response.IsSuccessStatusCode)
                {
                    // Opcional: notifica éxito al usuario
                    System.Diagnostics.Debug.WriteLine("Perfil actualizado correctamente");
                    // Redirigir a la pantalla principal pasando el token
                    await Shell.Current.GoToAsync($"//Principal?token={_token}");
                }
                else
                {
                    // Opcional: notifica error al usuario
                    System.Diagnostics.Debug.WriteLine("Error al actualizar el perfil");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Excepción al actualizar perfil: {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
