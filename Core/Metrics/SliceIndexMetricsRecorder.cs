using System;
using System.Diagnostics;
using App.Metrics;
using App.Metrics.Histogram;
using App.Metrics.Meter;

namespace Core.Metrics
{
    public class SliceIndexMetricsRecorder : ISliceIndexMetricsRecorder
    {
        private readonly IMetrics _metrics;
        private readonly Stopwatch _getStopwatch;
        private readonly Stopwatch _putStopwatch;

        public SliceIndexMetricsRecorder(IMetrics metrics)
        {
            _metrics = metrics;
            _getStopwatch = new Stopwatch();
            _putStopwatch = new Stopwatch();
        }

        private MeterOptions GetMeter => new MeterOptions() { Name = $"{typeof(LogSliceIndex).Name}.Get.Meter", MeasurementUnit = Unit.Events};
        private MeterOptions UpdateMeter => new MeterOptions() { Name = $"{typeof(LogSliceIndex).Name}.Update.Meter", MeasurementUnit = Unit.Events};
        private HistogramOptions GetHistogram => new HistogramOptions() { Name = $"{typeof(LogSliceIndex).Name}.Get.Hist", MeasurementUnit = Unit.Events};
        private HistogramOptions UpdateHistogram => new HistogramOptions() { Name = $"{typeof(LogSliceIndex).Name}.Update.Hist", MeasurementUnit = Unit.Events};
        
        public void GetStarted()
        {
            _getStopwatch.Restart();
        }

        public void GetFinished()
        {
            try
            {
                _getStopwatch.Stop();
                _metrics.Measure.Meter.Mark(GetMeter);
                _metrics.Measure.Histogram.Update(GetHistogram, _getStopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                //supress
            }
        }
        
        public void UpdatedStarted()
        {
            _putStopwatch.Restart();
        }

        public void UpdateFinished()
        {
            try
            {
                _putStopwatch.Stop();
                _metrics.Measure.Meter.Mark(UpdateMeter);
                _metrics.Measure.Histogram.Update(UpdateHistogram, _getStopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                //supress
            }
        }
    }
}