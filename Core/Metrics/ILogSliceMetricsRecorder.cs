namespace Core.Metrics
{
    public interface ILogSliceMetricsRecorder
    {
        void AppendStarted();
        void AppendFinished();
        void ContainsStarted();
        void ContainsFinished();
        void GetStarted();
        void GetFinished();
        
    }
}