using System.Windows;
using System.Windows.Controls;
using Checador_App_Wpf.Controllers;
using ControlDeCheckeo.Views;

namespace ControlDeCheckeo
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Instance = this; // Para acceso global
            LoadInitialView();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is CheckerView checkerView)
            {
                checkerView.DetenerLector();
            }

            AuthController.CerrarSesion();
            MainContent.Content = new LoginView();
        }

        private void LoadInitialView()
        {
            MainContent.Content = new LoginView();
        }

        public void CambiarVista(UserControl vista)
        {
            MainContent.Content = vista;
        }

        private void RRHH_Click(object sender, RoutedEventArgs e)
        {
            if (MainContent.Content is CheckerView checkerView)
            {
                checkerView.DetenerLector();
            }

            MainContent.Content = new RRHHView();
        }

        private void Vigilante_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CheckerView();
        }

        // 🔄 Métodos para controlar el loader global
        public void MostrarLoader(string mensaje = "Cargando...")
        {
            GlobalLoader.Visibility = Visibility.Visible;
            GlobalLoader.SetMensaje(mensaje);
        }

        public void OcultarLoader()
        {
            GlobalLoader.Visibility = Visibility.Collapsed;
        }

        // 🛑 Salida segura con contraseña
        private void CerrarConClave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PasswordDialog
            {
                Owner = this
            };

            if (dialog.ShowDialog() == true)
            {
                if (dialog.Password == "admin123") // Cambia esto por una clave segura real
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    MessageBox.Show("Contraseña incorrecta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
