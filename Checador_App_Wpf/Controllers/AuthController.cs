using Checador_App_Wpf.Models;
using Checador_App_Wpf.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Checador_App_Wpf.Controllers
{
    public static class AuthController
    {
        public static string Token { get; private set; }
        public static Usuario UsuarioActual { get; private set; }

        public static async Task<bool> IniciarSesion(string usuario, string clave)
        {
            var servicio = new LoginService();
            var respuesta = await servicio.LoginAsync(usuario, clave);

            if (respuesta == null)
            {
                Debug.WriteLine("❌ LoginService devolvió NULL (probablemente status code != 200 o error de conexión)");
                return false;
            }

            if (respuesta.usuario == null)
            {
                Debug.WriteLine("❌ LoginService devolvió objeto, pero 'usuario' es NULL.");
                return false;
            }

            Debug.WriteLine("✅ Login correcto. Usuario: " + respuesta.usuario.NombreUsuario);
            UsuarioActual = respuesta.usuario;
            Token = respuesta.token;
            return true;
        }
        public static void CerrarSesion()
        {
            UsuarioActual = null;
            Token = null;
        }

    }
}
