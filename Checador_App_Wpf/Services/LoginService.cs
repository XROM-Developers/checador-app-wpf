using Checador_App_Wpf.Models;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checador_App_Wpf.Services
{
    public class LoginRequest
    {
        public string usuarioSistema { get; set; }
        public string passwordUsuario { get; set; }
    }

    public class LoginResponse
    {
        public string token { get; set; }
        public Usuario usuario { get; set; }
    }

    public class LoginService
    {
        private readonly HttpClient _client;
        private readonly string baseUrl = "https://cm-backend.tpp.com.mx/v1/api/";

        public LoginService()
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(15) // ⏱️ Timeout de 15 segundos
            };
        }

        public async Task<LoginResponse?> LoginAsync(string usuario, string password)
        {
            var loginData = new LoginRequest
            {
                usuarioSistema = usuario,
                passwordUsuario = password
            };

            var json = JsonSerializer.Serialize(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _client.PostAsync("Auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();

                    Debug.WriteLine("📥 JSON crudo recibido:");
                    Debug.WriteLine(responseJson);

                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    try
                    {
                        var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, options);

                        if (loginResponse == null)
                        {
                            Debug.WriteLine("⚠️ Error: no se pudo deserializar LoginResponse.");
                        }

                        return loginResponse;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("💥 Excepción al deserializar LoginResponse: " + ex.Message);
                        return null;
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    Debug.WriteLine($"❌ Mensaje de error: {error}");
                    return null;
                }

            }
            catch (TaskCanceledException ex) when (!ex.CancellationToken.IsCancellationRequested)
            {
                Console.WriteLine("⏳ La solicitud de login excedió el tiempo de espera.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"💥 Error inesperado durante el login: {ex.Message}");
                return null;
            }
        }

    }
}
