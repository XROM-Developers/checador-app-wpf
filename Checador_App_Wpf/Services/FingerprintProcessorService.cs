// FingerprintProcessorService.cs
using DPFP;
using DPFP.Capture;
using DPFP.Processing;
using System;

namespace Checador_App_Wpf.Services
{
    public class FingerprintProcessorService
    {
        // Método para comparar dos huellas
        public bool CompareFingerprints(Sample sample1, Sample sample2)
        {
            var extractor = new FeatureExtraction();
            CaptureFeedback feedback1 = CaptureFeedback.None;
            CaptureFeedback feedback2 = CaptureFeedback.None;
            FeatureSet features1 = new FeatureSet();
            FeatureSet features2 = new FeatureSet();

            // Extraer las características de las dos muestras
            extractor.CreateFeatureSet(sample1, DataPurpose.Verification, ref feedback1, ref features1);
            extractor.CreateFeatureSet(sample2, DataPurpose.Verification, ref feedback2, ref features2);

            if (feedback1 == CaptureFeedback.Good && feedback2 == CaptureFeedback.Good)
            {
                // Aquí, podemos hacer la comparación manual
                var comparisonScore = CompareFeatureSets(features1, features2);

                return comparisonScore >= 1000;  // Si el puntaje de coincidencia es suficientemente alto, las huellas son iguales
            }

            return false;
        }

        // Método para comparar dos conjuntos de características
        private int CompareFeatureSets(FeatureSet features1, FeatureSet features2)
        {
            // Esta comparación puede ser más detallada dependiendo del SDK que estás usando.
            // Aquí puedes agregar un código para comparar las huellas a través de alguna métrica de similitud.

            // Esto es un ejemplo de comparación simple.
            return features1.GetHashCode() == features2.GetHashCode() ? 1000 : 0;  // Utiliza un puntaje para la comparación
        }
    }
}
