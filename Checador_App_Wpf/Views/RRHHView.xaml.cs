using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using FlashCap;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ControlDeCheckeo.Views
{
    public partial class RRHHView : UserControl
    {
        private Bitmap? _lastFrame;
        private CaptureDevice? _device;
        private Capture _fingerprintCapture;  // Instancia para captura de huellas
        private Enrollment _enroller;  // Instancia para manejar la inscripción de huellas

        public RRHHView()
        {
            InitializeComponent();
            Loaded += RRHHView_Loaded;
            Unloaded += RRHHView_Unloaded;
        }

        private async void RRHHView_Loaded(object sender, RoutedEventArgs e)
        {
            // Configuración de la cámara (sin cambios)
            var devices = new CaptureDevices();
            var descriptor = devices.EnumerateDescriptors().FirstOrDefault();

            if (descriptor != null)
            {
                var characteristic = descriptor.Characteristics
                    .FirstOrDefault(c => c.Width == 640 && c.Height == 480);

                _device = await descriptor.OpenAsync(characteristic, async bufferScope =>
                {
                    byte[] imageData = bufferScope.Buffer.ExtractImage();
                    using var ms = new MemoryStream(imageData);
                    var bitmap = new Bitmap(ms);

                    Dispatcher.Invoke(() =>
                    {
                        _lastFrame?.Dispose();
                        _lastFrame = new Bitmap(bitmap);
                        imgCamara.Source = ConvertBitmapToImageSource(bitmap);
                    });
                });

                await _device.StartAsync();
            }
            else
            {
                MessageBox.Show("No se encontró ninguna cámara.");
            }

            // Inicialización del lector de huellas
            _fingerprintCapture = new Capture();
            _enroller = new Enrollment();
            _fingerprintCapture.EventHandler = new DPFPHandler(this);
            _fingerprintCapture.StartCapture();
        }

        private async void RRHHView_Unloaded(object sender, RoutedEventArgs e)
        {
            // Detener la captura de la huella y la cámara
            if (_device != null)
            {
                await _device.StopAsync();
                _device.Dispose();
            }

            if (_fingerprintCapture != null)
            {
                _fingerprintCapture.StopCapture();
                _fingerprintCapture.Dispose();
            }
        }

        // Método para manejar la huella digital capturada
        private class DPFPHandler : DPFP.Capture.EventHandler
        {
            private readonly RRHHView _view;

            public DPFPHandler(RRHHView view)
            {
                _view = view;
            }

            public void OnComplete(object capture, string readerSerialNumber, Sample sample)
            {
                _view.Dispatcher.Invoke(() =>
                {
                    var features = _view.ExtractFeatures(sample, DataPurpose.Enrollment);
                    if (features != null && _view._enroller != null)
                    {
                        _view._enroller.AddFeatures(features);

                        // Si se ha capturado la huella completa
                        if (_view._enroller.TemplateStatus == Enrollment.Status.Ready)
                        {
                            _view._fingerprintCapture?.StopCapture();
                            _view.txtEstadoHuella.Text = "Huella registrada con éxito.";
                            _view.MostrarHuella(sample);
                        }
                        else if (_view._enroller.TemplateStatus == Enrollment.Status.Failed)
                        {
                            _view.txtEstadoHuella.Text = "Error al capturar huella.";
                            _view._enroller.Clear();
                            _view._fingerprintCapture?.StopCapture();
                        }
                    }
                    else
                    {
                        _view.txtEstadoHuella.Text = "Muestra inválida. Intente nuevamente.";
                    }
                });
            }

            public void OnFingerGone(object capture, string readerSerialNumber) { }
            public void OnFingerTouch(object capture, string readerSerialNumber) { }
            public void OnReaderConnect(object capture, string readerSerialNumber) { }
            public void OnReaderDisconnect(object capture, string readerSerialNumber) { }
            public void OnSampleQuality(object capture, string readerSerialNumber, CaptureFeedback captureFeedback) { }
        }

        // Extrae las características de la huella
        private FeatureSet ExtractFeatures(Sample sample, DataPurpose purpose)
        {
            var extractor = new FeatureExtraction();
            CaptureFeedback feedback = CaptureFeedback.None;
            FeatureSet features = new FeatureSet();
            extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        // Muestra la huella en el control Image
        private void MostrarHuella(Sample sample)
        {
            var convert = new SampleConversion();
            Bitmap bmp = null;
            convert.ConvertToPicture(sample, ref bmp);

            if (bmp != null)
            {
                imgHuella.Source = ConvertBitmapToImageSource(bmp);  // Actualiza el control de huella
            }
        }

        // Convierte un Bitmap a ImageSource para WPF
        private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            using var memory = new MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        // Captura la huella en un archivo
        private void btnTomarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (_lastFrame != null)
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "foto_usuario.jpg");
                _lastFrame.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                MessageBox.Show($"Foto guardada: {path}");
            }
        }
    }
}
