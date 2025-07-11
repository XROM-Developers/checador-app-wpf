using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Checador_App_Wpf.Controllers;
using Newtonsoft.Json;
using System.Diagnostics;
using Checador_App_Wpf.Models;
using static Checador_App_Wpf.Models.EmpleadoHuella;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class FingerprintService
{
    private readonly HttpClient _client;
    private readonly string baseUrl = "https://cm-backend.tpp.com.mx/v1/api/";

    public FingerprintService(string userId, string sessionToken)
    {
        Debug.WriteLine("🛠 Inicializando FingerprintService");

        _client = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(200)
        };

        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {AuthController.Token}");
        _client.DefaultRequestHeaders.Add("IdUsuario", AuthController.UsuarioActual.IdUsuario.ToString() ?? "");

        Debug.WriteLine($"🔧 Base URL: {baseUrl}");
    }

    public async Task<string> RegisterFingerprintAsync(int idUsuario, byte[] archivoHuella, string dedo)
    {
        try
        {
            Debug.WriteLine("📤 Enviando huella como byte[]...");

            var body = new
            {
                idUsuario = idUsuario,
                archivoHuella = archivoHuella,
                dedo = dedo
            };

            string json = JsonConvert.SerializeObject(body);
            var encoding = new UTF8Encoding(false);
            byte[] contentBytes = encoding.GetBytes(json);
            var byteContent = new ByteArrayContent(contentBytes);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            Debug.WriteLine("🧾 Petición completa a enviar:");
            Debug.WriteLine($"➡ Endpoint: {_client.BaseAddress}Huella");
            Debug.WriteLine("➡ Headers:");
            foreach (var header in _client.DefaultRequestHeaders)
                Debug.WriteLine($"   {header.Key}: {string.Join(", ", header.Value)}");
            Debug.WriteLine("➡ Body JSON:");
            Debug.WriteLine(json);

            var response = await _client.PostAsync("Huella", byteContent);
            string responseContent = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($"📥 Respuesta del servidor: {responseContent}");

            return response.IsSuccessStatusCode
                ? responseContent
                : $"❌ Error {response.StatusCode}: {responseContent}";
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Error al enviar huella: " + ex);
            return $"ERROR_LOCAL: {ex.Message}";
        }
    }




    public async Task<AccessResponse?> CheckAccessAsync(string archivoHuellaBase64)
    {
        try
        {
            var body = new
            {
                archivoHuella = archivoHuellaBase64
            };

            string json = JsonConvert.SerializeObject(body);
            var encoding = new UTF8Encoding(false);
            byte[] contentBytes = encoding.GetBytes(json);
            var byteContent = new ByteArrayContent(contentBytes);
            byteContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            Debug.WriteLine("📤 Enviando huella para comprobar acceso...");

            var response = await _client.PostAsync("Huella/ComprobarAcceso", byteContent);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"❌ Error de respuesta: {response.StatusCode}");
                return null;
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("📥 Respuesta recibida: " + responseContent);

            var accessResponse = JsonConvert.DeserializeObject<AccessResponse>(responseContent);

            return accessResponse;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Error al comprobar acceso: " + ex.Message);
            return null;
        }
    }

    public async Task<List<EmpleadoHuella>?> GetUsuariosConHuellasAsync(int idUsuarioAccion)
    {
        try
        {
            Debug.WriteLine("📡 Solicitando usuarios con huellas...");

            var response = await _client.GetAsync($"Huella/usuarios?idUsuarioAccion={idUsuarioAccion}");

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"❌ Error al obtener usuarios: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            Debug.WriteLine("📥 JSON recibido: ");

            var usuarios = JsonConvert.DeserializeObject<List<EmpleadoHuella>>(json);
            return usuarios;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Excepción al obtener usuarios con huellas: " + ex.Message);
            return null;
        }
    }

    public async Task<List<Huella>> getHuellas(string idUsuario)
    {
        try
        {
            var response = await _client.GetAsync($"Huella/detalle/{idUsuario}");

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error al obtener huellas: {response.StatusCode}");
                return new List<Huella>();
            }

            var json = await response.Content.ReadAsStringAsync();
            var huellas = JsonConvert.DeserializeObject<List<Huella>>(json);

            return huellas ?? new List<Huella>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Excepción en getHuellas: {ex.Message}");
            return new List<Huella>();
        }
    }



    public async Task<AccesoResult?> RegistrarAccesoNormalAsync(int idUsuario, int idUsuarioAccion)
    {
        try
        {
            var payload = new
            {
                idUsuario = idUsuario,
                idUsuarioAccion = idUsuarioAccion
            };

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync("Acceso/normal", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseBody);

                if (data != null && data.ContainsKey("mensaje") && data.ContainsKey("color"))
                {
                    var mensaje = data["mensaje"].ToString();

                    int color = 0;
                    var colorObj = data["color"];

                    if (colorObj is long)
                        color = Convert.ToInt32((long)colorObj);
                    else if (colorObj is int)
                        color = (int)colorObj;
                    else
                        int.TryParse(colorObj.ToString(), out color);

                    return new AccesoResult
                    {
                        Mensaje = mensaje,
                        Color = color
                    };
                }

                Debug.WriteLine("⚠️ Respuesta incompleta (faltan campos 'mensaje' o 'color').");
                return null;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(error);
                var mensaje = data["mensaje"].ToString();
                var color = 3;

                return new AccesoResult
                {
                    Mensaje = mensaje,
                    Color = color
                };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine("❌ Excepción: " + ex.Message);
            return null;
        }
    }

}
