using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DPFP.Capture;
using DPFP.Processing;
using DPFP;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using System.Drawing;
using System.IO;

namespace ControlDeCheckeo.Components.Fingerprints
{
    /// <summary>
    /// Lógica de interacción para FingerprintCaptureControl.xaml
    /// </summary>
    public partial class FingerprintCaptureControl : UserControl
    {

private Capture? _fingerprintCapture;
    private Enrollment? _enroller;

    private void IniciarCaptura_Click(object sender, RoutedEventArgs e)
    {
        _enroller = new Enrollment();
        _fingerprintCapture = new Capture();

        if (_fingerprintCapture != null)
        {
            _fingerprintCapture.EventHandler = new DPFPHandler(this);
            _fingerprintCapture.StartCapture();

            txtStatus.Text = "Coloque el dedo en el lector...";
        }
    }

    // Clase interna para manejar eventos de DigitalPersona
    private class DPFPHandler : DPFP.Capture.EventHandler
    {
        private readonly FingerprintCaptureControl _control;

        public DPFPHandler(FingerprintCaptureControl control)
        {
            _control = control;
        }

        public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
        {
            _control.Dispatcher.Invoke(() =>
            {
                var features = _control.ExtractFeatures(Sample, DataPurpose.Enrollment);
                if (features != null && _control._enroller is not null)
                {
                    _control._enroller.AddFeatures(features);

                    _control.txtStatus.Text = $"Muestras: {3 - _control._enroller.FeaturesNeeded}/3 capturadas.";

                    if (_control._enroller.TemplateStatus == Enrollment.Status.Ready)
                    {
                        _control._fingerprintCapture?.StopCapture();
                        _control.txtStatus.Text = "Huella registrada con éxito.";
                        _control.MostrarHuella(Sample);
                    }
                    else if (_control._enroller.TemplateStatus == Enrollment.Status.Failed)
                    {
                        _control.txtStatus.Text = "Error al capturar huella.";
                        _control._enroller.Clear();
                        _control._fingerprintCapture?.StopCapture();
                    }
                }
                else
                {
                    _control.txtStatus.Text = "Muestra inválida. Intente nuevamente.";
                }
            });
        }

        public void OnFingerGone(object capture, string readerSerialNumber) { }
        public void OnFingerTouch(object capture, string readerSerialNumber) { }
        public void OnReaderConnect(object capture, string readerSerialNumber) { }
        public void OnReaderDisconnect(object capture, string readerSerialNumber) { }
        public void OnSampleQuality(object capture, string readerSerialNumber, CaptureFeedback captureFeedback) { }
}

// Extrae características de una muestra
private FeatureSet ExtractFeatures(Sample sample, DataPurpose purpose)
        {
            var extractor = new FeatureExtraction();
            CaptureFeedback feedback = CaptureFeedback.None;
            FeatureSet features = new FeatureSet();


            extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        // Convierte muestra en imagen para preview
        private void MostrarHuella(Sample sample)
        {
            var convert = new SampleConversion();
            Bitmap bmp = null;
            convert.ConvertToPicture(sample, ref bmp);

            if (bmp != null)
            {
                imgHuellaPreview.Source = ConvertBitmapToImageSource(bmp);
            }
        }

        // Convierte Bitmap a ImageSource (WPF)
        private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            ms.Position = 0;

            var image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = ms;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();

            return image;
        }

        public FingerprintCaptureControl()
        {
            InitializeComponent();
        }
    }
}
