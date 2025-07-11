using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using System.Windows.Media.Animation;

namespace Checador_App_Wpf.Components.Fingerprints
{
    public partial class FingerAnimationControl : UserControl
    {
        private readonly Dictionary<string, Ellipse> _dedos = new();
        private string _dedoActivo = null; // ✅ Guardamos cuál está activo

        public event Action<string> FingerClicked;

        public FingerAnimationControl()
        {
            InitializeComponent();
            InicializarDedos();
        }

        private void InicializarDedos()
        {
            for (int i = 1; i <= 5; i++)
            {
                var left = this.FindName($"Left{i}") as Ellipse;
                var right = this.FindName($"Right{i}") as Ellipse;

                if (left != null)
                {
                    _dedos[$"Left{i}"] = left;
                    left.MouseDown += OnFingerClicked;
                }

                if (right != null)
                {
                    _dedos[$"Right{i}"] = right;
                    right.MouseDown += OnFingerClicked;
                }
            }
        }

        private void OnFingerClicked(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Ellipse clickedEllipse)
            {
                foreach (var kvp in _dedos)
                {
                    if (kvp.Value == clickedEllipse)
                    {
                        FingerClicked?.Invoke(kvp.Key);
                        break;
                    }
                }
            }
        }

        public enum FingerState
        {
            Normal,
            Processing,
            Complete
        }

        public void SetFingerState(string fingerKey, FingerState state)
        {
            if (!_dedos.TryGetValue(fingerKey, out var ellipse)) return;

            Color color = state switch
            {
                FingerState.Normal => Color.FromRgb(52, 152, 219),      // Azul
                FingerState.Processing => Color.FromRgb(231, 76, 60),   // Rojo
                FingerState.Complete => Color.FromRgb(46, 204, 113),    // Verde
                _ => Colors.Gray
            };

            var brush = new SolidColorBrush(color);

            // Detener animación previa
            if (_dedoActivo != null && _dedoActivo != fingerKey && _dedos.TryGetValue(_dedoActivo, out var anterior))
            {
                if (anterior.Fill is SolidColorBrush oldBrush)
                    oldBrush.BeginAnimation(SolidColorBrush.ColorProperty, null);

                anterior.Fill = new SolidColorBrush(Color.FromRgb(52, 152, 219)); // volver a azul
            }

            // Iniciar animación si es "Processing"
            if (state == FingerState.Processing)
            {
                var animation = new ColorAnimation
                {
                    From = Colors.White,
                    To = color,
                    Duration = TimeSpan.FromSeconds(0.5),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever
                };

                brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                _dedoActivo = fingerKey; // ✅ marcar como activo
            }
            else
            {
                brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                if (_dedoActivo == fingerKey)
                    _dedoActivo = null;
            }

            ellipse.Fill = brush;
        }

        public void ResetAll()
        {
            foreach (var key in _dedos.Keys)
            {
                SetFingerState(key, FingerState.Normal);
            }
            _dedoActivo = null;
        }
    }
}
