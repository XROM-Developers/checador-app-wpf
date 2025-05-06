using DPFP;
using DPFP.Processing;
using System;
using System.Windows;
using Checador_App_Wpf.Services;
using DPFP.Capture;
using System.Windows.Controls;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerprintControl : UserControl
    {
        private readonly FingerprintCaptureService _captureService;
        private readonly FingerprintEnrollmentService _enrollmentService;
        private int currentFingerIndex = 1;  // Inicia con el pulgar

        public FingerprintControl()
        {
            InitializeComponent();
            _captureService = new FingerprintCaptureService();
            _enrollmentService = new FingerprintEnrollmentService();

            _captureService.FingerprintCaptured += CaptureService_FingerprintCaptured;
        }

        private void CaptureService_FingerprintCaptured(object sender, Sample e)
        {
            var features = ExtractFeatures(e); // Extraer las características de la huella
            if (features != null)
            {
                _enrollmentService.AddFeatures(e, currentFingerIndex); // Agregar huella al servicio de inscripción

                // Mostrar el estado de las muestras capturadas
                txtStatus.Text = $"Muestras: {_enrollmentService.FeaturesNeeded}/10 capturadas.";

                // Resaltar el dedo correspondiente en la animación
                fingerprintAnimationControl.HighlightFinger(currentFingerIndex);

                // Verificar si la inscripción está lista
                if (_enrollmentService.IsEnrollmentComplete())
                {
                    txtStatus.Text = "Huella registrada con éxito.";
                    _captureService.StopCapture();
                }
                else
                {
                    // Cambiar al siguiente dedo
                    currentFingerIndex++;
                    if (currentFingerIndex > 5)
                    {
                        currentFingerIndex = 1; // Reiniciar si llega al último dedo
                    }
                }
            }
            else
            {
                txtStatus.Text = "Muestra inválida. Intente nuevamente.";
            }
        }

        // Método para extraer características de la huella
        private FeatureSet ExtractFeatures(Sample sample)
        {
            var extractor = new FeatureExtraction();
            CaptureFeedback feedback = CaptureFeedback.None;
            FeatureSet features = new FeatureSet();
            extractor.CreateFeatureSet(sample, DataPurpose.Enrollment, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        // Inicia la captura de huellas
        private void IniciarCaptura_Click(object sender, RoutedEventArgs e)
        {
            _captureService.StartCapture();
            txtStatus.Text = "Coloque el dedo en el lector...";
        }
    }
}
