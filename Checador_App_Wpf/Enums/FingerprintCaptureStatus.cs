// Enums/FingerprintCaptureStatus.cs
namespace Checador_App_Wpf.Enums
{
    public enum FingerprintCaptureStatus
    {
        InProgress,  // En proceso de captura
        Completed,   // Captura completada con éxito
        Error,       // Error durante la captura
        NotDetected  // No se detectó huella
    }
}
