using AzureStorageEmulator.NET.Common;
using PeriodicLogger;
using Serilog;
using Serilog.Events;
using SerilogTracing;
using SimpleApiSink;

namespace AzureStorageEmulator.NET
{
    public static class Serilog
    {
        public static void Setup(Settings settings)
        {
            // Set up Serilog logging
            if (!settings.LogSettings.LogToFrontEnd)
            {
                if (settings.LogSettings.DetailedLogging)
                {
                    Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                        .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                        .Enrich.WithProperty("Application", "Example")
                        .WriteTo.PeriodicLoggerSink(settings.LogSettings.BatchSeconds)
                        .CreateLogger();

                    using IDisposable listener = new ActivityListenerConfiguration()
                        .Instrument.AspNetCoreRequests()
                        .TraceToSharedLogger();
                }

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.PeriodicLoggerSink(settings.LogSettings.BatchSeconds)
                    .CreateLogger();
            }
            else
            {
                if (settings.LogSettings.LogUrl is null)
                {
                    return;
                }

                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.Console()
                    .WriteTo.ApiSink(settings.LogSettings.LogUrl)
                    .CreateLogger();
                Log.Information($"Logging to {settings.LogSettings.LogUrl}");
            }
        }
    }
}