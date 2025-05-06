// Models/Fingerprint.cs
using DPFP;
using System;

namespace Checador_App_Wpf.Models
{
    public class Fingerprint
    {
        public Sample Sample { get; set; } // La muestra de la huella
        public byte[] Image { get; set; }  // Imagen de la huella (puede ser un array de bytes o en formato base64)
        public DateTime CapturedAt { get; set; } // Fecha de captura

        // Constructor para inicializar la huella
        public Fingerprint(Sample sample, byte[] image)
        {
            Sample = sample;
            Image = image;
            CapturedAt = DateTime.Now;
        }
    }
}
