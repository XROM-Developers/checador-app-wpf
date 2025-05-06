// ViewModels/FingerprintViewModel.cs
using Checador_App_Wpf.Enums;
using Checador_App_Wpf.Models;
using ControlDeCheckeo.Helpers;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace Checador_App_Wpf.ViewModels
{
    public class FingerprintViewModel : INotifyPropertyChanged
    {
        private FingerprintCaptureStatus _captureStatus;
        public FingerprintCaptureStatus CaptureStatus
        {
            get => _captureStatus;
            set
            {
                if (_captureStatus != value)
                {
                    _captureStatus = value;
                    OnPropertyChanged(nameof(CaptureStatus));
                }
            }
        }

        private Fingerprint? _fingerprint;  // Cambiar a nullable
        public Fingerprint? Fingerprint  // Cambiar la propiedad a nullable
        {
            get => _fingerprint;
            set
            {
                if (_fingerprint != value)
                {
                    _fingerprint = value;
                    OnPropertyChanged(nameof(Fingerprint));
                }
            }
        }

        public ICommand StartCaptureCommand { get; set; }
        public ICommand StopCaptureCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };  // Inicializar el evento

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public FingerprintViewModel()
        {
            // Inicialización de comandos
            StartCaptureCommand = new RelayCommand(param => ExecuteStartCapture());
            StopCaptureCommand = new RelayCommand(param => ExecuteStopCapture());
        }

        private void ExecuteStartCapture()
        {
            // Lógica para iniciar la captura
            CaptureStatus = FingerprintCaptureStatus.InProgress;
            // Aquí puedes iniciar la captura de la huella y actualizar el estado
        }

        private void ExecuteStopCapture()
        {
            // Lógica para detener la captura
            CaptureStatus = FingerprintCaptureStatus.Completed;
            // Aquí puedes manejar la finalización de la captura
        }

    }
}
