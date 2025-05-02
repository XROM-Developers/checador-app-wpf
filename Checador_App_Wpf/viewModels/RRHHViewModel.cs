using System.Windows.Input;
using ControlDeCheckeo.Helpers;

namespace ControlDeCheckeo.ViewModels
{
    public class RRHHViewModel : ViewModelBase
    {
        private string _nombre;
        public string Nombre
        {
            get => _nombre;
            set => SetProperty(ref _nombre, value);
        }

        private string _documento;
        public string Documento
        {
            get => _documento;
            set => SetProperty(ref _documento, value);
        }

        public ICommand CapturarHuellaCommand { get; }

        public RRHHViewModel()
        {
            CapturarHuellaCommand = new RelayCommand(CapturarHuella);
        }

        private void CapturarHuella(object obj)
        {
            // Futuro: Capturar huella y guardar en base de datos
        }
    }
}
