namespace Core.Metrics
{
    public interface ILogSliceMetricsRecorder
    {
        void AppendStarted();
        void AppendFinished();
        void ContainsStarted();
        void ContainsFinished();
        void RemoveStarted();
        void RemoveFinished();
        void GetStarted();
        void GetFinished();
        
    }
}