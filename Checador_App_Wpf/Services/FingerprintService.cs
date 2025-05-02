public class FingerprintService
{
    private DPFP.Processing.Enrollment _enroller;
    private List<DPFP.Sample> _muestras;

    public event Action<DPFP.Template> OnTemplateGenerado;
    public event Action<int> OnMuestraAgregada;

    public FingerprintService()
    {
        _enroller = new DPFP.Processing.Enrollment();
        _muestras = new List<DPFP.Sample>();
    }

    public void ProcesarMuestra(DPFP.Sample sample)
    {
        var features = ExtractFeatures(sample, DPFP.Processing.DataPurpose.Enrollment);
        if (features != null)
        {
            _enroller.AddFeatures(features);
            _muestras.Add(sample);
            OnMuestraAgregada?.Invoke(_muestras.Count);

            if (_enroller.TemplateStatus == DPFP.Processing.Enrollment.Status.Ready)
            {
                OnTemplateGenerado?.Invoke(_enroller.Template);
            }
        }
    }

    private DPFP.FeatureSet ExtractFeatures(DPFP.Sample sample, DPFP.Processing.DataPurpose purpose)
    {
        var extractor = new DPFP.Processing.FeatureExtraction();

        DPFP.Capture.CaptureFeedback feedback = DPFP.Capture.CaptureFeedback.None;
        DPFP.FeatureSet features = null;

        extractor.CreateFeatureSet(sample, purpose, ref feedback, ref features);

        return feedback == DPFP.Capture.CaptureFeedback.Good ? features : null;
    }

}
