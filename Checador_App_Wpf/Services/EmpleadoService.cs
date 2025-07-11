using Checador_App_Wpf.Controllers;
using Checador_App_Wpf.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace Checador_App_Wpf.Services
{
    public class EmpleadoService
    {
        private readonly HttpClient _client;
        private readonly string baseUrl = "https://cm-backend.tpp.com.mx/v1/api/";

        public EmpleadoService()
        {
            _client = new HttpClient { BaseAddress = new Uri(baseUrl) };

            // Asignar headers globales con datos del usuario autenticado
            _client.DefaultRequestHeaders.Add("Idusuario", AuthController.UsuarioActual.IdUsuario.ToString());
            _client.DefaultRequestHeaders.Add("x-session-token", AuthController.Token);
        }

        public async Task<List<Empleado>?> GetEmpleadosAsync()
        {
            var response = await _client.GetAsync("Empleados");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<Empleado>>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            Console.WriteLine($"❌ Error al obtener empleados: {response.StatusCode}");
            return null;
        }

        public async Task<EmpleadoDetalle?> GetEmpleadoAsync(int idEmpleado)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"Empleados/Detalle/{idEmpleado}");

            // Configurar headers personalizados
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthController.Token);
            request.Headers.Add("IdUsuario", AuthController.UsuarioActual.IdUsuario.ToString());

            var response = await _client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<EmpleadoDetalle>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            Debug.WriteLine($"❌ Error al obtener empleado con ID {idEmpleado}: {response.StatusCode}");
            return null;
        }
    }
}
