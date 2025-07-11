using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Checador_App_Wpf.Controllers;
using Checador_App_Wpf.Models;
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using System.Linq;
using System.Collections.Generic;
using DPFP.Verification;
using Newtonsoft.Json;
using Checador_App_Wpf.Services;
using System.Media;

namespace ControlDeCheckeo.Views
{
    public partial class CheckerView : UserControl
    {
        private readonly FingerprintService _fingerprintService;
        private bool _escuchando = true;
        private TaskCompletionSource<Sample> _sampleCaptured;
        private Capture _capturador;
        private List<EmpleadoHuella> _usuariosConHuellas;
        private ImageBrush FotoPerfilBrush;

        public CheckerView()
        {
            InitializeComponent();
            _fingerprintService = new FingerprintService(AuthController.UsuarioActual.IdUsuario.ToString(), AuthController.Token);
            StartClock();
            _ = CargarHuellasAsync();
            _ = IniciarEscuchaContinuaAsync();
            _ = CargarDatosDelGuardiaAsync();
            FotoPerfilBrush = (ImageBrush)this.Resources["FotoPerfilBrushKey"];
            FotoPerfilBrush = (ImageBrush)this.Resources["FotoPerfilBrushKey"];

        }

        private async Task CargarDatosDelGuardiaAsync()
        {
            try
            {
                var empleadoService = new EmpleadoService();
                var detalle = await empleadoService.GetEmpleadoAsync(AuthController.UsuarioActual.IdUsuario);

                if (detalle != null)
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Actualizar nombre del guardia
                        lblGuardia.Text = $"Guardia: {detalle.NombreUsuario} {detalle.ApellidoPaternoUsuario}";
                    });

                    if (!string.IsNullOrEmpty(detalle.FotoUsuarioURL) &&
                        Resources["FotoGuardiaBrushKey"] is ImageBrush brush)
                    {
                        var imagen = new BitmapImage();
                        imagen.BeginInit();
                        imagen.UriSource = new Uri(detalle.FotoUsuarioURL, UriKind.Absolute);
                        imagen.CacheOption = BitmapCacheOption.OnLoad;
                        imagen.EndInit();

                        Dispatcher.Invoke(() =>
                        {
                            brush.ImageSource = imagen;
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al cargar datos del guardia: {ex.Message}");
            }
        }



        private async Task CargarHuellasAsync()
        {
            try
            {
                Dispatcher.Invoke(() => MainWindow.Instance.MostrarLoader("Descargando huellas..."));

                Debug.WriteLine("🌐 Descargando huellas desde el servidor (sin usar caché)...");
                _usuariosConHuellas = await _fingerprintService.GetUsuariosConHuellasAsync(AuthController.UsuarioActual.IdUsuario);

                if (_usuariosConHuellas != null && _usuariosConHuellas.Any())
                {
                    Debug.WriteLine($"✅ {_usuariosConHuellas.Count} usuarios con huellas recibidas.");

                    foreach (var usuario in _usuariosConHuellas)
                    {
                        foreach (var huella in usuario.huellas)
                        {
                            try
                            {
                                using var stream = new MemoryStream(Convert.FromBase64String(huella.archivoHuella));
                                huella.Template = new Template(stream);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"❌ Error al deserializar template: {ex.Message}");
                            }
                        }
                    }
                }
                else
                {
                    Debug.WriteLine("⚠️ No se recibieron huellas desde el servidor.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error general en carga de huellas: " + ex.Message);
            }
            finally
            {
                Dispatcher.Invoke(() => MainWindow.Instance.OcultarLoader());
            }
        }

        private async Task IniciarEscuchaContinuaAsync()
        {
            while (_escuchando)
            {
                try
                {
                    var sample = await ObtenerSampleAsync();

                    if (sample != null)
                    {
                        var features = ExtractFeatures(sample);
                        if (features != null)
                        {
                            await ComprobarHuellaAsync(features);
                            await Task.Delay(5000);
                        }
                    }
                    else
                    {
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("❌ Error en la escucha: " + ex.Message);
                    lblResultado.Text = "Error interno";
                    lblResultado.Foreground = new SolidColorBrush(Colors.OrangeRed);
                    await Task.Delay(2000);
                }
            }
        }

        private async Task<Sample?> ObtenerSampleAsync()
        {
            _sampleCaptured = new TaskCompletionSource<Sample>();
            _capturador = new Capture { EventHandler = new CaptureHandler(this) };

            _capturador.StartCapture();
            lblResultado.Text = "Coloque su dedo en el lector...";

            var sample = await _sampleCaptured.Task;
            _capturador.StopCapture();
            return sample;
        }

        public void SampleCaptured(Sample? sample)
        {
            _sampleCaptured?.TrySetResult(sample);
        }

        public FeatureSet? ExtractFeatures(Sample sample)
        {
            var extractor = new FeatureExtraction();
            var features = new FeatureSet();
            var feedback = CaptureFeedback.None;

            extractor.CreateFeatureSet(sample, DataPurpose.Verification, ref feedback, ref features);
            return feedback == CaptureFeedback.Good ? features : null;
        }

        private async Task ComprobarHuellaAsync(FeatureSet features)
        {
            try
            {
                if (_usuariosConHuellas == null || !_usuariosConHuellas.Any())
                {
                    lblResultado.Text = "No hay datos para comparar";
                    lblResultado.Foreground = new SolidColorBrush(Colors.DarkOrange);
                    return;
                }

                var coincidencia = _usuariosConHuellas
                    .SelectMany(u => u.huellas, (usuario, huella) => new { usuario, huella })
                    .FirstOrDefault(par =>
                    {
                        try
                        {
                            if (par.huella.Template == null) return false;

                            var verificador = new Verification();
                            var result = new Verification.Result();
                            verificador.Verify(features, par.huella.Template, ref result);

                            return result.Verified;
                        }
                        catch
                        {
                            return false;
                        }
                    });

                if (coincidencia != null)
                {
                    await EjecutarAccionPorUsuario(coincidencia.usuario.nombreCompleto, coincidencia.usuario.idUsuario, coincidencia.usuario);
                }
                else
                {
                    var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "error.wav");
                    var player = new SoundPlayer(path);
                    player.Load();
                    player.Play();
                    lblResultado.Text = "Acceso denegado";
                    lblResultado.Foreground = new SolidColorBrush(Colors.Red);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("❌ Error en la comparación de huellas: " + ex.Message);
                lblResultado.Text = "Error interno";
                lblResultado.Foreground = new SolidColorBrush(Colors.OrangeRed);
            }
        }

        public void DetenerLector()
        {
            try
            {
                _escuchando = false;
                _capturador?.StopCapture();
                Debug.WriteLine("🛑 Lector de huellas detenido.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error al detener lector: {ex.Message}");
            }
        }

        private async Task EjecutarAccionPorUsuario(string nombre, int idUsuario, EmpleadoHuella usuario)
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Sounds", "confirm.wav");
            var player = new SoundPlayer(path);
            player.Load();
            player.Play();
            UserName.Text = nombre;
            lblUltima.Text = DateTime.Now.ToString("hh:mm tt");
            lblResultado.Text = "Acceso concedido";
            lblResultado.Foreground = new SolidColorBrush(Colors.Green);

            Puesto.Text = usuario.puesto;

            if (!string.IsNullOrEmpty(usuario.fotoPerfil))
            {
                try
                {
                    var imagen = new BitmapImage();
                    imagen.BeginInit();
                    imagen.UriSource = new Uri(usuario.fotoPerfil, UriKind.Absolute);
                    imagen.CacheOption = BitmapCacheOption.OnLoad;
                    imagen.EndInit();

                    FotoPerfilBrush.ImageSource = imagen; // ✅ Actualiza el ImageBrush existente
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"⚠️ No se pudo cargar la foto: {ex.Message}");
                }
            }

            Debug.WriteLine($"✅ Acción ejecutada para el usuario: {usuario.nombreCompleto}");

            var resultado = await _fingerprintService.RegistrarAccesoNormalAsync(idUsuario, AuthController.UsuarioActual.IdUsuario);

            if (!string.IsNullOrEmpty(resultado.Mensaje))
            {
                lblResultado.Text = resultado.Mensaje;
                switch (resultado.Color)
                {
                    case 1:
                        lblResultado.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case 2:
                        lblResultado.Foreground = new SolidColorBrush(Colors.OrangeRed);
                        break;
                    case 3:
                        lblResultado.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    default:
                        lblResultado.Foreground = new SolidColorBrush(Colors.Yellow);
                        break;
                }
                
            }
        }



        private void StartClock()
        {
            var timer = new System.Windows.Threading.DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += (s, e) =>
            {
                lblHora.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();
        }

        public class CaptureHandler : DPFP.Capture.EventHandler
        {
            private readonly CheckerView _view;

            public CaptureHandler(CheckerView view)
            {
                _view = view;
            }

            public void OnComplete(object Capture, string ReaderSerialNumber, Sample sample)
            {
                _view.SampleCaptured(sample);
            }

            public void OnFingerTouch(object Capture, string ReaderSerialNumber) { }
            public void OnFingerGone(object Capture, string ReaderSerialNumber) { }
            public void OnReaderConnect(object Capture, string ReaderSerialNumber) { }
            public void OnReaderDisconnect(object Capture, string ReaderSerialNumber) { }
            public void OnSampleQuality(object Capture, string ReaderSerialNumber, CaptureFeedback feedback) { }
        }
    }
}
