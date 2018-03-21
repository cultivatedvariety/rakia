using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Meter;

namespace Core.Metrics
{
    public class MetricsRegistry
    {
        public MeterOptions SliceIndexGetCounter => new MeterOptions()
        {
            Name = "SliceIndex.Get",
            MeasurementUnit = Unit.Events
        };

        public MeterOptions SliceIndexPutCounter => new MeterOptions()
        {
            Name = "SliceIndex.Put",
            MeasurementUnit = Unit.Events
        };
    }
}