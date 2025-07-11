using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerPreviewControl : UserControl
    {
        public FingerPreviewControl()
        {
            InitializeComponent();
        }

        // Actualiza la imagen de huella
        public void SetPreviewImage(BitmapImage image)
        {
            imgFingerprint.Source = image;
        }

        // Actualiza el texto de estado
        public void SetStatusText(string message)
        {
            txtStatus.Text = message;
        }
    }
}
