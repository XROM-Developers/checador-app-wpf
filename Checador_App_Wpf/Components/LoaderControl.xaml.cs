using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Checador_App_Wpf.Components
{
    public partial class LoaderControl : UserControl
    {
        public LoaderControl()
        {
            InitializeComponent();
            IniciarAnimaciones();
        }

        public void IniciarAnimaciones()
        {
            // Rotación continua
            var rotateAnimation = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = new Duration(TimeSpan.FromSeconds(1)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoaderRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, rotateAnimation);

            // Escalado pulsante
            var scaleUpDown = new DoubleAnimation
            {
                From = 1.0,
                To = 1.15,
                AutoReverse = true,
                Duration = new Duration(TimeSpan.FromSeconds(0.6)),
                RepeatBehavior = RepeatBehavior.Forever
            };
            LoaderScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, scaleUpDown);
            LoaderScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, scaleUpDown);
        }

        public void DetenerAnimacion()
        {
            LoaderRotate.BeginAnimation(System.Windows.Media.RotateTransform.AngleProperty, null);
            LoaderScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleXProperty, null);
            LoaderScale.BeginAnimation(System.Windows.Media.ScaleTransform.ScaleYProperty, null);
        }

        public void SetMensaje(string texto)
        {
            Mensaje.Text = texto;
        }
    }
}
