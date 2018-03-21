namespace Core.Metrics
{
    /**
     * Represents a instance capable of recording the metrics produced by a SliceIndex
     */
    public interface ISliceIndexMetricsRecorder
    {
        void IndexGetStarted();
        void IndexGetFinished();
        void IndexPutStarted();
        void IndexPutFinished();
    }
}