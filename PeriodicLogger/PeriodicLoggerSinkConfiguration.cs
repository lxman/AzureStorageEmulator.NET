using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace PeriodicLogger
{
    public static class PeriodicLoggerSinkConfiguration
    {
        public static LoggerConfiguration PeriodicLoggerSink(this LoggerSinkConfiguration loggerConfiguration, int period)
        {
            PeriodicLoggerSink sink = new();

            PeriodicBatchingSinkOptions batchingOptions = new()
            {
                BatchSizeLimit = 100,
                Period = TimeSpan.FromSeconds(period),
                EagerlyEmitFirstEvent = true,
                QueueLimit = 10000
            };

            PeriodicBatchingSink periodicBatchingSink = new(sink, batchingOptions);

            return loggerConfiguration.Sink(periodicBatchingSink);
        }
    }
}