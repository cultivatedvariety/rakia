using Core.Metrics;

namespace Core
{
    public class LogSliceFactory : ILogSliceFactory
    {
        private readonly ILogSliceMetricsRecorder _logSliceMetricsRecorder;
        private readonly ISliceIndexMetricsRecorder _indexMetricsRecorder;

        public LogSliceFactory(ILogSliceMetricsRecorder logSliceMetricsRecorder, ISliceIndexMetricsRecorder indexMetricsRecorder)
        {
            _logSliceMetricsRecorder = logSliceMetricsRecorder;
            _indexMetricsRecorder = indexMetricsRecorder;
        }

        public ILogSlice CreateSlice(string filePath)
        {
            var idxFilePath = string.Format("{0}.idx", filePath);
            LogSliceIndex logSliceIndex = new LogSliceIndex(idxFilePath, _indexMetricsRecorder);
            return new LogSlice(filePath, logSliceIndex, _logSliceMetricsRecorder);
        }
    }
}