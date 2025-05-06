using DPFP;
using DPFP.Processing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Checador_App_Wpf.Services
{
    // Servicio para registrar huellas dactilares
    public class FingerprintEnrollmentService
    {
        private Dictionary<int, List<Sample>> _fingerprints; // Almacenamos las huellas por dedo
        private const int MaxSamplesPerFinger = 2; // Número máximo de huellas por dedo

        // Necesitamos 10 huellas (2 por dedo, 5 dedos)
        public int FeaturesNeeded => MaxSamplesPerFinger * 5 - TotalCapturedSamples; // Huellas necesarias

        // Total de huellas capturadas en todos los dedos
        public int TotalCapturedSamples => _fingerprints.Values.Sum(f => f.Count);

        // Estado de la inscripción
        public TemplateStatus EnrollmentStatus { get; private set; }

        public FingerprintEnrollmentService()
        {
            _fingerprints = new Dictionary<int, List<Sample>>();
            EnrollmentStatus = TemplateStatus.Pending; // Inicialmente está pendiente
        }

        // Agregar huellas por dedo
        public void AddFeatures(Sample sample, int fingerIndex)
        {
            if (!_fingerprints.ContainsKey(fingerIndex))
            {
                _fingerprints[fingerIndex] = new List<Sample>();
            }

            // Verificar que no se añadan más de 2 huellas por dedo
            if (_fingerprints[fingerIndex].Count < MaxSamplesPerFinger)
            {
                _fingerprints[fingerIndex].Add(sample);  // Agregar huella para el dedo específico
            }
            else
            {
                // Lanza una excepción si se intenta agregar más de 2 huellas por dedo
                throw new InvalidOperationException($"Ya se han registrado 2 huellas para el dedo {fingerIndex}.");
            }

            // Verificar si la inscripción está completa (2 huellas por dedo, 10 huellas en total)
            if (IsEnrollmentComplete())
            {
                EnrollmentStatus = TemplateStatus.Ready; // Inscripción lista
            }
        }

        // Verificar si la inscripción está completa (2 huellas por dedo, 10 huellas en total)
        public bool IsEnrollmentComplete()
        {
            return _fingerprints.Values.All(f => f.Count == MaxSamplesPerFinger);  // Verificar que haya 2 huellas por dedo
        }

        // Limpiar las huellas registradas
        public void Clear()
        {
            _fingerprints.Clear();
            EnrollmentStatus = TemplateStatus.Pending; // Restablecer el estado de inscripción a pendiente
        }
    }

    // Enum para los posibles estados de la inscripción
    public enum TemplateStatus
    {
        Pending,  // La inscripción está pendiente
        Ready,    // La inscripción está completa
        Failed    // La inscripción ha fallado
    }
}
