using System.Windows;
using System.Windows.Controls;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerprintCaptureControl : UserControl
    {
        public FingerprintCaptureControl()
        {
            InitializeComponent();
        }

        // Método para actualizar la UI con el estado de la huella
        public void UpdateCaptureStatus(string status)
        {
            txtCaptureStatus.Text = status;
        }

        // Método para actualizar la imagen de la huella
        public void UpdateFingerprintImage(System.Windows.Media.Imaging.BitmapImage image)
        {
            imgCapturePreview.Source = image;
        }
    }
}
