using System.Windows.Input;
using ControlDeCheckeo.Helpers;

namespace ControlDeCheckeo.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _usuario;
        public string Usuario
        {
            get => _usuario;
            set => SetProperty(ref _usuario, value);
        }

        private string _contrasena;
        public string Contrasena
        {
            get => _contrasena;
            set => SetProperty(ref _contrasena, value);
        }

        public ICommand IniciarSesionCommand { get; }

        public LoginViewModel()
        {
            IniciarSesionCommand = new RelayCommand(EjecutarLogin);
        }

        private void EjecutarLogin(object parameter)
        {
            // Lógica futura: validar contra base de datos
        }
    }
}
