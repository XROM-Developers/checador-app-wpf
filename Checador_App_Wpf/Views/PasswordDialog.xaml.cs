using System.Windows;

namespace ControlDeCheckeo.Views
{
    public partial class PasswordDialog : Window
    {
        public string Password => passwordBox.Password;

        public PasswordDialog()
        {
            InitializeComponent();
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
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
