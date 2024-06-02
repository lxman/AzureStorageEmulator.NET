using Serilog.Events;
using Serilog.Formatting;
using Serilog.Sinks.PeriodicBatching;
using Serilog.Templates.Themes;
using SerilogTracing.Expressions;

namespace AzureStorageEmulator.NET.Periodic_Logger
{
    public class PeriodicLoggerSink : IBatchedLogEventSink
    {
        private readonly ITextFormatter _formatter = Formatters.CreateConsoleTextFormatter(TemplateTheme.Code);

        public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
        {
            foreach (LogEvent logEvent in batch)
            {
                Console.Write($"{logEvent.Timestamp:HH:mm:ss.ffffff} - ");
                _formatter.Format(logEvent, Console.Out);
            }
            return Task.CompletedTask;
        }

        public Task OnEmptyBatchAsync() => Task.CompletedTask;
    }
}