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

        public RRHHView()
        {
            InitializeComponent();
            Loaded += RRHHView_Loaded;
            Unloaded += RRHHView_Unloaded;
        }

        private async void RRHHView_Loaded(object sender, RoutedEventArgs e)
        {
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
        }

        private async void RRHHView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                await _device.StopAsync();
                _device.Dispose();
            }
        }

        private void btnTomarFoto_Click(object sender, RoutedEventArgs e)
        {
            if (_lastFrame != null)
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "foto_usuario.jpg");
                _lastFrame.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                MessageBox.Show($"Foto guardada: {path}");
            }
        }

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
    }
}
