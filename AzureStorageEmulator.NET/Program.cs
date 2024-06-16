using AppliedQueueList;
using AzureStorageEmulator.NET.Authentication;
using AzureStorageEmulator.NET.Blob.Services;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Table.Services;
using AzureStorageEmulator.NET.XmlSerialization;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using PeriodicLogger;
using Serilog;
using Serilog.Events;
using SerilogTracing;
using TableStorage;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected

namespace AzureStorageEmulator.NET
{
    public static class Program
    {
        private const bool DetailedLogging = false;
        private const int BatchSeconds = 2;
        private const bool LogGetMessages = false;
        private const bool CreateAppliedQueues = true;

        public static async Task<int> Main(string[] args)
        {
            if (DetailedLogging)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Warning)
                    .MinimumLevel.Override("Microsoft.AspNetCore.Routing", LogEventLevel.Warning)
                    .Enrich.WithProperty("Application", "Example")
                    .WriteTo.PeriodicLoggerSink(BatchSeconds)
                    .CreateLogger();

                using IDisposable listener = new ActivityListenerConfiguration()
                    .Instrument.AspNetCoreRequests()
                    .TraceToSharedLogger();
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .WriteTo.PeriodicLoggerSink(BatchSeconds)
                .CreateLogger();

            Log.Information("Starting up");

            try
            {
                QueueSettings settings = new() { LogGetMessages = LogGetMessages };

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers().AddXmlSerializerFormatters();
                builder.Services.AddSerilog();
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSingleton<IFifoService, ConcurrentQueueService>();
                builder.Services.AddSingleton<IQueueSettings>(settings);

                builder.Services.AddScoped<IQueueService, QueueService>();
                builder.Services.AddScoped<IBlobService, BlobService>();
                builder.Services.AddScoped<IAuthenticator, QueueSharedKeyAuthenticator>();
                builder.Services.AddScoped<IXmlSerializer<MessageList>, XmlSerializer<MessageList>>();
                builder.Services.AddScoped<IXmlSerializer<QueueEnumerationResults>, XmlSerializer<QueueEnumerationResults>>();
                builder.Services.AddSingleton<ITableStorage, TableStorage.TableStorage>();
                builder.Services.AddScoped<ITableStorageService, TableStorageService>();

                WebApplication app = builder.Build();

                app.Urls.Add("http://localhost:10000");
                app.Urls.Add("http://localhost:10001");
                app.Urls.Add("http://localhost:10002");

                // Configure the HTTP request pipeline.
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                Log.Information("Adding queues");

                if (CreateAppliedQueues) SetupQueues(app);

                Log.Information($"{Queues.Names.Count} queues added");

                await app.RunAsync();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during startup");
                return 1;
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }

        private static void SetupQueues(WebApplication app)
        {
            IFifoService fifoService = app.Services.GetRequiredService<IFifoService>();
            foreach (string queue in Queues.Names)
            {
                fifoService.AddQueueAsync(queue);
            }
        }
    }
}