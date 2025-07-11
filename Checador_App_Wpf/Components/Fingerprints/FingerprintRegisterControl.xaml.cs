using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerprintRegisterControl : UserControl
    {
        private Capture _capture;
        private FeatureExtraction _extractor;
        private Dictionary<string, string> _fingerprintPaths;
        private int _samplesPerFinger = 5;
        private int _samplesCaptured = 0;
        private string _tempDirectory;
        private string _currentFingerKey = null;
        private string _currentFingerName = null; 
        private Enrollment _enrollment = new();


        private readonly Dictionary<string, string> _fingerMap = new()
        {
            { "Left1", "Pulgar izquierdo" },
            { "Left2", "Índice izquierdo" },
            { "Left3", "Medio izquierdo" },
            { "Left4", "Anular izquierdo" },
            { "Left5", "Meñique izquierdo" },
            { "Right1", "Pulgar derecho" },
            { "Right2", "Índice derecho" },
            { "Right3", "Medio derecho" },
            { "Right4", "Anular derecho" },
            { "Right5", "Meñique derecho" }
        };

        public int UserId { get; set; }
        public string SessionToken { get; set; }
        public string PhotoPath { get; set; }

        public void resetFingers()
        {
            fingerAnimationControl.ResetAll();
        }

        private async Task CargarHuellasRegistradas()
        {
            try
            {
                Debug.WriteLine("📥 Cargando huellas...");
                var service = new FingerprintService(UserId.ToString(), SessionToken);
                var huellas = await service.getHuellas(UserId.ToString());

                if (huellas == null || huellas.Count == 0)
                {
                    Debug.WriteLine("ℹ️ No hay huellas registradas.");
                    return;
                }

                foreach (var huella in huellas)
                {
                    // Buscar la clave del dedo en el diccionario
                    var fingerKey = _fingerMap.FirstOrDefault(x => x.Value == huella.Dedo).Key;

                    if (!string.IsNullOrEmpty(fingerKey))
                    {
                        fingerAnimationControl.SetFingerState(fingerKey, FingerAnimationControl.FingerState.Complete);
                        Debug.WriteLine($"🟢 Huella registrada detectada: {huella.Dedo} ({fingerKey})");
                    }
                    else
                    {
                        Debug.WriteLine($"⚠️ Dedo no reconocido en el diccionario: {huella.Dedo}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al cargar huellas registradas: {ex.Message}");
            }
        }



        public FingerprintRegisterControl()
        {
            InitializeComponent();

            _capture = new Capture();
            _extractor = new FeatureExtraction();
            _fingerprintPaths = new Dictionary<string, string>();
            _capture.EventHandler = new CaptureHandler(this);
            _tempDirectory = Path.Combine(Path.GetTempPath(), "fingerprints");
            Directory.CreateDirectory(_tempDirectory);
            _ = CargarHuellasRegistradas();
            fingerAnimationControl.FingerClicked += OnFingerSelected;
        }

        private void OnFingerSelected(string fingerKey)
        {
            if (!_fingerMap.TryGetValue(fingerKey, out string fingerName))
            {
                Debug.WriteLine($"❌ Dedo no reconocido: {fingerKey}");
                return;
            }

            _currentFingerKey = fingerKey;
            _currentFingerName = fingerName;
            _samplesCaptured = 0;
            _enrollment.Clear();

            try
            {
                _capture.StartCapture();
                Debug.WriteLine("🟢 Captura iniciada al seleccionar dedo.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al iniciar lector: {ex.Message}");
                UpdateStatus("Error al iniciar lector.");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                fingerAnimationControl.SetFingerState(fingerKey, FingerAnimationControl.FingerState.Processing);
                UpdateStatus($"Capturando {fingerName}, muestra 1 de {_samplesPerFinger}");
            });
        }

        private void OnSampleCaptured(Sample sample)
        {
            Debug.WriteLine("🟢 Muestra recibida");

            Dispatcher.Invoke(() =>
            {
                var imagenHuella = ConvertSampleToBitmapImage(sample);
                if (imagenHuella != null)
                {
                    imgHuellaPreview.Source = imagenHuella;
                }
            });

            if (string.IsNullOrEmpty(_currentFingerKey) || string.IsNullOrEmpty(_currentFingerName))
            {
                Debug.WriteLine("⚠️ Muestra ignorada: no hay dedo seleccionado.");
                return;
            }

            var features = ExtractFeatures(sample);
            if (features == null)
            {
                UpdateStatus("Muestra inválida. Intente nuevamente.");
                return;
            }

            _enrollment.AddFeatures(features);
            _samplesCaptured++;

            Dispatcher.Invoke(() =>
            {
                fingerAnimationControl.SetFingerState(_currentFingerKey, FingerAnimationControl.FingerState.Processing);
                UpdateStatus($"Muestras aceptadas: {_samplesCaptured}");
            });

            switch (_enrollment.TemplateStatus)
            {
                case Enrollment.Status.Ready:
                    StopCapture();

                    using (var stream = new MemoryStream())
                    {
                        _enrollment.Template.Serialize(stream);
                        byte[] templateBytes = stream.ToArray();

                        _ = SendFingerprintToServer(_currentFingerName, templateBytes);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        fingerAnimationControl.SetFingerState(_currentFingerKey, FingerAnimationControl.FingerState.Complete);
                        UpdateStatus($"Huella de {_currentFingerName} registrada con {_samplesCaptured} muestras.");
                    });

                    _enrollment.Clear();
                    _currentFingerKey = null;
                    _currentFingerName = null;
                    break;

                case Enrollment.Status.Failed:
                    StopCapture();
                    UpdateStatus("❌ Falló la creación de la plantilla. Intente de nuevo.");
                    _enrollment.Clear();
                    break;
            }
        }


        private FeatureSet ExtractFeatures(Sample sample)
        {
            CaptureFeedback feedback = CaptureFeedback.None;
            FeatureSet features = new FeatureSet();
            _extractor.CreateFeatureSet(sample, DataPurpose.Enrollment, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        private async Task SendFingerprintToServer(string dedoNombre, byte[] templateBytes)
        {
            try
            {
                var service = new FingerprintService(UserId.ToString(), SessionToken);
                await service.RegisterFingerprintAsync(UserId, templateBytes, dedoNombre); // ✅ Enviar byte[]
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al enviar huella de {dedoNombre}: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private BitmapImage ConvertSampleToBitmapImage(Sample sample)
        {
            try
            {
                var convertor = new SampleConversion();
                Bitmap bitmap = new Bitmap(248, 292); // tamaño por defecto
                convertor.ConvertToPicture(sample, ref bitmap);

                using var memory = new MemoryStream();
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error al convertir sample a imagen: " + ex.Message);
                return null;
            }
        }

        private void StopCapture()
        {
            try
            {
                _capture?.StopCapture();
                Debug.WriteLine("🛑 Captura detenida.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al detener la captura: {ex.Message}");
            }
        }

        private void UpdateStatus(string message)
        {
            Dispatcher.Invoke(() => txtStatus.Text = message);
        
        }

        public async Task PrepararRegistroParaUsuario(int userId)
        {
            // 🧠 Actualiza ID
            UserId = userId;

            // 🧼 Limpiar estado previo
            _enrollment.Clear();
            _samplesCaptured = 0;
            _currentFingerKey = null;
            _currentFingerName = null;
            StopCapture();
            resetFingers();

            // 🔄 Recargar huellas del usuario nuevo
            await CargarHuellasRegistradas();

            // 🎯 Reset visual y status
            Dispatcher.Invoke(() =>
            {
                imgHuellaPreview.Source = null;
                UpdateStatus("Seleccione un dedo para registrar");
            });
        }




        private class CaptureHandler : DPFP.Capture.EventHandler
        {
            private readonly FingerprintRegisterControl _control;

            public CaptureHandler(FingerprintRegisterControl control)
            {
                _control = control;
            }

            public void OnComplete(object Capture, string ReaderSerialNumber, Sample Sample)
            {
                _control.OnSampleCaptured(Sample);
            }

            public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
            public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
            public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
            public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
            public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback CaptureFeedback) { }
        }
    }
}
