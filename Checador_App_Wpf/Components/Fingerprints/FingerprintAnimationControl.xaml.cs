using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerprintAnimationControl : UserControl
    {
        public FingerprintAnimationControl()
        {
            InitializeComponent();
        }

        // Método para iluminar el dedo correspondiente
        public void HighlightFinger(int fingerIndex)
        {
            // Resetear todos los dedos a gris
            ResetFingers();

            // Iluminar el dedo correspondiente
            switch (fingerIndex)
            {
                case 1:
                    AnimateFinger(Thumb);
                    break;
                case 2:
                    AnimateFinger(Index);
                    break;
                case 3:
                    AnimateFinger(Middle);
                    break;
                case 4:
                    AnimateFinger(Ring);
                    break;
                case 5:
                    AnimateFinger(Pinkie);
                    break;
            }
        }

        // Método para animar el dedo
        private void AnimateFinger(Ellipse finger)
        {
            var animation = new ColorAnimation
            {
                From = Colors.Gray,
                To = Colors.Green,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                AutoReverse = true
            };

            finger.Fill.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }

        // Método para resetear todos los dedos
        private void ResetFingers()
        {
            Thumb.Fill = new SolidColorBrush(Colors.Gray);
            Index.Fill = new SolidColorBrush(Colors.Gray);
            Middle.Fill = new SolidColorBrush(Colors.Gray);
            Ring.Fill = new SolidColorBrush(Colors.Gray);
            Pinkie.Fill = new SolidColorBrush(Colors.Gray);
        }
    }
}
