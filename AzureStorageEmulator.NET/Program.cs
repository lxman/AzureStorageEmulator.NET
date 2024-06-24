using AzureStorageEmulator.NET.Authorization;
using AzureStorageEmulator.NET.Blob.Models;
using AzureStorageEmulator.NET.Blob.Services;
using AzureStorageEmulator.NET.Blob.Xml;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Common.HeaderManagement;
using AzureStorageEmulator.NET.Exceptions;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Table.Services;
using AzureStorageEmulator.NET.XmlSerialization;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using PeriodicLogger;
using QueueList;
using Serilog;
using Serilog.Events;
using SerilogTracing;
using TableStorage;
using Metadata = AzureStorageEmulator.NET.Blob.Models.Metadata;

// ReSharper disable UnusedParameter.Local

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
                Settings settings = new()
                {
                    QueueSettings = { LogGetMessages = LogGetMessages },
                    TableSettings = { LogGetMessages = LogGetMessages },
                    BlobSettings = { LogGetMessages = LogGetMessages }
                };

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers()
                    .AddXmlSerializerFormatters();
                builder.Services.AddSerilog();
                builder.Services.AddEndpointsApiExplorer();

                // Settings
                builder.Services.AddSingleton<ISettings>(settings);

                // Services
                builder.Services.AddSingleton<IFifoService, ConcurrentQueueService>();
                builder.Services.AddScoped<IQueueService, QueueService>();
                builder.Services.AddScoped<ITableService, TableService>();
                builder.Services.AddScoped<IBlobService, BlobService>();
                builder.Services.AddSingleton<ITableStorage, TableStorage.TableStorage>();
                builder.Services.AddTransient<HeaderManager>();
                builder.Services.AddSingleton<IBlobRoot, BlobRoot>();

                // Serializers
                builder.Services.AddScoped<IXmlSerializer<MessageList>, XmlSerializer<MessageList>>();
                builder.Services
                    .AddScoped<IXmlSerializer<QueueEnumerationResults>, XmlSerializer<QueueEnumerationResults>>();
                builder.Services.AddScoped<IXmlSerializer<Metadata>, XmlSerializer<Metadata>>();
                builder.Services.AddScoped<IXmlSerializer<ContainerEnumerationResults>, XmlSerializer<ContainerEnumerationResults>>();

                // Middleware
                builder.Services.AddTransient<Authorizer>();
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

                WebApplication app = builder.Build();

                app.Urls.Add("http://localhost:10000");
                app.Urls.Add("http://localhost:10001");
                app.Urls.Add("http://localhost:10002");

                // Configure the HTTP request pipeline.
                app.UseHttpsRedirection();

                // The use of the empty options is due to web api bug. If you leave it out, the application will not start.
                // https://github.com/dotnet/aspnetcore/issues/51888
                app.UseExceptionHandler(opt => { });

                app.UseMiddleware<Authorizer>();
                app.UseMiddleware<HeaderManager>();

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