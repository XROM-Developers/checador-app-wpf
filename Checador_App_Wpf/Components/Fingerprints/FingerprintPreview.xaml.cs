using System.Windows;
using System.Windows.Controls;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerprintPreview : UserControl
    {
        public FingerprintPreview()
        {
            InitializeComponent();
        }

        // Método para actualizar la imagen de la huella
        public void UpdateFingerprintImage(System.Windows.Media.Imaging.BitmapImage image)
        {
            imgPreview.Source = image;
        }

        // Método para actualizar el estado
        public void UpdatePreviewStatus(string status)
        {
            txtPreviewStatus.Text = status;
        }
    }
}
