using System.Windows;
using System.Windows.Input;

namespace ControlDeCheckeo.Views
{
    public partial class PasswordDialog : Window
    {
        public string Password => passwordBox.Password;

        public PasswordDialog()
        {
            InitializeComponent();

            // Dar foco automáticamente al PasswordBox al abrir el diálogo
            Loaded += (s, e) => passwordBox.Focus();
        }

        private void Aceptar_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(passwordBox.Password))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor ingresa una contraseña.", "Campo requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
                passwordBox.Focus();
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Aceptar_Click(sender, new RoutedEventArgs());
            }
        }
    }
}
