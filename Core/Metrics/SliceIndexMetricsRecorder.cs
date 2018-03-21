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

        private MeterOptions GetMeter => new MeterOptions() { Name = "SliceIndex.Get.Meter", MeasurementUnit = Unit.Events};
        private MeterOptions PutMeter => new MeterOptions() { Name = "SliceIndex.Put.Meter", MeasurementUnit = Unit.Events};
        private HistogramOptions GetHistogram => new HistogramOptions() { Name = "SliceIndex.Get.Hist", MeasurementUnit = Unit.Events};
        private HistogramOptions PutHistogram => new HistogramOptions() { Name = "SliceIndex.Put.Hist", MeasurementUnit = Unit.Events};
        
        public void IndexGetStarted()
        {
            _getStopwatch.Restart();
        }

        public void IndexGetFinished()
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
        
        public void IndexPutStarted()
        {
            _putStopwatch.Restart();
        }

        public void IndexPutFinished()
        {
            try
            {
                _putStopwatch.Stop();
                _metrics.Measure.Meter.Mark(PutMeter);
                _metrics.Measure.Histogram.Update(PutHistogram, _getStopwatch.ElapsedMilliseconds);
            }
            catch (Exception)
            {
                //supress
            }
        }
    }
}