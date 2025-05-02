using System.Windows.Input;
using ControlDeCheckeo.Helpers;

namespace ControlDeCheckeo.ViewModels
{
    public class CheckerViewModel : ViewModelBase
    {
        private string _resultado;
        public string Resultado
        {
            get => _resultado;
            set => SetProperty(ref _resultado, value);
        }

        public ICommand EscanearHuellaCommand { get; }

        public CheckerViewModel()
        {
            Resultado = "Esperando...";
            EscanearHuellaCommand = new RelayCommand(EscanearHuella);
        }

        private void EscanearHuella(object obj)
        {
            // Futuro: Integrar con Biometría
            Resultado = "Verificado correctamente.";
        }
    }
}
