using System.Windows;
using ControlDeCheckeo.Views; // Asegúrate de tener este namespace si tus vistas están ahí

namespace ControlDeCheckeo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadInitialView();
        }

        private void LoadInitialView()
        {
            // Carga vista inicial (puede ser LoginView o CheckerView temporalmente)
            MainContent.Content = new CheckerView(); // O puedes usar: new RRHHView();
        }

        private void RRHH_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new RRHHView();
        }

        private void Vigilante_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new CheckerView();
        }
    }
}
