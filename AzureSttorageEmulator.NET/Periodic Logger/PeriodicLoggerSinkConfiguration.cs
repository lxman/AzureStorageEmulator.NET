using Serilog;
using Serilog.Configuration;
using Serilog.Sinks.PeriodicBatching;

namespace AzureStorageEmulator.NET.Periodic_Logger
{
    public static class PeriodicLoggerSinkConfiguration
    {
        public static LoggerConfiguration PeriodicLoggerSink(this LoggerSinkConfiguration loggerConfiguration, int period)
        {
            var sink = new PeriodicLoggerSink();

            var batchingOptions = new PeriodicBatchingSinkOptions
            {
                BatchSizeLimit = 100,
                Period = TimeSpan.FromSeconds(period),
                EagerlyEmitFirstEvent = true,
                QueueLimit = 10000
            };

            var periodicBatchingSink = new PeriodicBatchingSink(sink, batchingOptions);

            return loggerConfiguration.Sink(periodicBatchingSink);
        }
    }
}