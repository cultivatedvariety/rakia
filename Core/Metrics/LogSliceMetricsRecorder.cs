using System.Diagnostics;
using App.Metrics;
using App.Metrics.Histogram;
using App.Metrics.Meter;

namespace Core.Metrics
{
    public class LogSliceMetricsRecorder : ILogSliceMetricsRecorder
    {
        private readonly IMetrics _metrics;
        private readonly ISliceIndexMetricsRecorder _sliceIndexMetricsRecorder;
        private readonly Stopwatch _appendStopwatch;
        private readonly Stopwatch _contansStopwatch;
        private readonly Stopwatch _removeStopwatch;
        private readonly Stopwatch _updateStopwatch;
        
        private MeterOptions AppendMeter => new MeterOptions() { Name = $"{typeof(LogSlice).Name}.Append.Meter", MeasurementUnit = Unit.Events};
        private MeterOptions ContainsMeter => new MeterOptions() { Name = $"{typeof(LogSlice).Name}.Contains.Meter", MeasurementUnit = Unit.Events};
        private MeterOptions RemoveMeter => new MeterOptions() { Name = $"{typeof(LogSlice).Name}.Remove.Meter", MeasurementUnit = Unit.Events};
        private MeterOptions GetMeter => new MeterOptions() { Name = $"{typeof(LogSlice).Name}.Get.Meter", MeasurementUnit = Unit.Events};
        private HistogramOptions AppendHistogram => new HistogramOptions() { Name = $"{typeof(LogSlice).Name}.Append.Hist", MeasurementUnit = Unit.Events};
        private HistogramOptions ContainsHistogram => new HistogramOptions() { Name = $"{typeof(LogSlice).Name}.Contains.Hist", MeasurementUnit = Unit.Events};
        private HistogramOptions RemoveHistogram => new HistogramOptions() { Name = $"{typeof(LogSlice).Name}.Remove.Hist", MeasurementUnit = Unit.Events};
        private HistogramOptions GetHistogram => new HistogramOptions() { Name = $"{typeof(LogSlice).Name}.Get.Hist", MeasurementUnit = Unit.Events};


        public LogSliceMetricsRecorder(IMetrics metrics, ISliceIndexMetricsRecorder sliceIndexMetricsRecorder)
        {
            _metrics = metrics;
            _sliceIndexMetricsRecorder = sliceIndexMetricsRecorder;
            _appendStopwatch = new Stopwatch();
            _contansStopwatch = new Stopwatch();
            _removeStopwatch = new Stopwatch();
            _updateStopwatch = new Stopwatch();
        }


        public void AppendStarted()
        {
            _appendStopwatch.Restart();
        }

        public void AppendFinished()
        {
            _appendStopwatch.Stop();
            _metrics.Measure.Meter.Mark(AppendMeter);
            _metrics.Measure.Histogram.Update(AppendHistogram, _appendStopwatch.ElapsedMilliseconds);
            
        }

        public void ContainsStarted()
        {
            _contansStopwatch.Restart();
        }

        public void ContainsFinished()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveStarted()
        {
            throw new System.NotImplementedException();
        }

        public void RemoveFinished()
        {
            throw new System.NotImplementedException();
        }

        public void GetStarted()
        {
            throw new System.NotImplementedException();
        }

        public void GetFinished()
        {
            throw new System.NotImplementedException();
        }
    }
}