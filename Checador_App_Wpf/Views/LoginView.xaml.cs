using Checador_App_Wpf.Controllers;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ControlDeCheckeo.Views
{
    public partial class LoginView : UserControl
    {
        public LoginView()
        {
            InitializeComponent();
        }

        private void LoginField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Ejecuta la lógica como si hicieras clic en el botón
                BtnLogin_Click(sender, new RoutedEventArgs());
            }
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text.Trim();
            string clave = txtClave.Password.Trim();

            // 🚨 Validación de campos vacíos
            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show("Por favor, ingresa tu usuario.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(clave))
            {
                MessageBox.Show("Por favor, ingresa tu contraseña.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtClave.Focus();
                return;
            }

            MainWindow.Instance.MostrarLoader("Verificando credenciales...");

            var success = await AuthController.IniciarSesion(usuario, clave);

            MainWindow.Instance.OcultarLoader();

            if (success)
            {
                var modulos = AuthController.UsuarioActual.Modulos;

                if (modulos.Any(m => m.ModuloId == 12))
                {
                    MainWindow.Instance.CambiarVista(new RRHHView());
                }
                else if (modulos.Any(m => m.ModuloId == 13))
                {
                    MainWindow.Instance.CambiarVista(new CheckerView());
                }
                else
                {
                    MessageBox.Show("No tiene acceso a ningún módulo compatible.");
                }
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas.");
            }
        }
    }
}
