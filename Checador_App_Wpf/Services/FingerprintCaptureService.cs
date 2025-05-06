// FingerprintCaptureService.cs
using DPFP;
using DPFP.Capture;
using System;

namespace Checador_App_Wpf.Services
{
    // FingerprintCaptureService.cs
    public class FingerprintCaptureService
    {
        private readonly Capture _fingerprintCapture;
        public event EventHandler<Sample> FingerprintCaptured;  // Evento que se dispara cuando se captura una huella

        private Dictionary<int, List<Sample>> _fingerprints; // Para almacenar las huellas por dedo

        public FingerprintCaptureService()
        {
            _fingerprintCapture = new Capture();
            _fingerprints = new Dictionary<int, List<Sample>>();
        }

        // Inicia la captura de huellas
        public void StartCapture()
        {
            if (_fingerprintCapture != null)
            {
                _fingerprintCapture.EventHandler = new DPFPHandler(this);
                _fingerprintCapture.StartCapture();
            }
        }

        // Detiene la captura de huellas
        public void StopCapture()
        {
            _fingerprintCapture?.StopCapture();
        }

        // Manejador de eventos de captura
        private class DPFPHandler : DPFP.Capture.EventHandler
        {
            private FingerprintCaptureService _service;

            public DPFPHandler(FingerprintCaptureService service)
            {
                _service = service;
            }

            private int _fingerIndex = 1;  // Empezamos con el pulgar izquierdo

            public void OnComplete(object capture, string readerSerialNumber, Sample sample)
            {
                // Si ya tenemos 2 huellas para este dedo, pasamos al siguiente dedo
                if (_service._fingerprints.ContainsKey(_fingerIndex))
                {
                    if (_service._fingerprints[_fingerIndex].Count < 2)
                    {
                        _service._fingerprints[_fingerIndex].Add(sample);
                    }
                    else
                    {
                        // Cambiar al siguiente dedo (índice)
                        _fingerIndex++;
                        if (_fingerIndex <= 10)  // Asegurarse de no superar los 10 dedos
                        {
                            _service._fingerprints.Add(_fingerIndex, new List<Sample>());
                        }
                    }
                }
                else
                {
                    _service._fingerprints.Add(_fingerIndex, new List<Sample>());
                    _service._fingerprints[_fingerIndex].Add(sample);
                }

                // Notificar la captura de huella
                _service.FingerprintCaptured?.Invoke(this, sample);
            }

            public void OnFingerGone(object capture, string readerSerialNumber) { }
            public void OnFingerTouch(object capture, string readerSerialNumber) { }
            public void OnReaderConnect(object capture, string readerSerialNumber) { }
            public void OnReaderDisconnect(object capture, string readerSerialNumber) { }
            public void OnSampleQuality(object capture, string readerSerialNumber, CaptureFeedback captureFeedback) { }
        }
    }

}
