using AzureStorageEmulator.NET.Assembly_Attributes.Readers;
using AzureStorageEmulator.NET.Authorization;
using AzureStorageEmulator.NET.Blob.Models;
using AzureStorageEmulator.NET.Blob.Services;
using AzureStorageEmulator.NET.Blob.Xml;
using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Common.HeaderManagement;
using AzureStorageEmulator.NET.Exceptions;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Models.MessageResponseLists;
using AzureStorageEmulator.NET.Queue.Services;
using AzureStorageEmulator.NET.Table.Services;
using AzureStorageEmulator.NET.XmlSerialization;
using AzureStorageEmulator.NET.XmlSerialization.Queue;
using QueueList;
using Serilog;
using TableStorage;
using Metadata = AzureStorageEmulator.NET.Blob.Models.Metadata;

// ReSharper disable UnusedParameter.Local

namespace AzureStorageEmulator.NET
{
    public static class Program
    {
        private const bool DetailedLogging = false;
        private const int BatchSeconds = 2;
        private const bool LogGetMessages = false;
        private const bool CreateQueues = true;

        public static async Task<int> Main(string[] args)
        {
            Settings settings = new()
            {
                LogSettings = { BatchSeconds = BatchSeconds, DetailedLogging = DetailedLogging },
                QueueSettings = { LogGetMessages = LogGetMessages },
                TableSettings = { LogGetMessages = LogGetMessages },
                BlobSettings = { LogGetMessages = LogGetMessages }
            };

            if (args is ["true", _])
            {
                settings.LogSettings.LogUrl = new Uri($"http://127.0.0.1:{Convert.ToInt32(args[1])}");
                settings.LogSettings.LogToFrontEnd = true;
            }

            Serilog.Setup(settings);

            Log.Information("Starting up");
            Log.Information($"Build time for this module is {BuildTime.GetBuildTime().ToLocalTime()}");

            try
            {
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
                builder.Services
                    .AddScoped<IXmlSerializer<QueueEnumerationResults>, XmlSerializer<QueueEnumerationResults>>();
                builder.Services.AddScoped<IXmlSerializer<Metadata>, XmlSerializer<Metadata>>();
                builder.Services.AddScoped<IXmlSerializer<ContainerEnumerationResults>, XmlSerializer<ContainerEnumerationResults>>();
                builder.Services.AddScoped<IXmlSerializer<GetMessagesResponseList>, XmlSerializer<GetMessagesResponseList>>();
                builder.Services.AddScoped<IXmlSerializer<PeekMessageResponseList>, XmlSerializer<PeekMessageResponseList>>();
                builder.Services.AddScoped<IXmlSerializer<PutMessageResponseList>, XmlSerializer<PutMessageResponseList>>();

                // Middleware
                builder.Services.AddTransient<Authorizer>();
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

                WebApplication app = builder.Build();
                app.Urls.Add("http://localhost:10000");
                app.Urls.Add("http://localhost:10001");
                app.Urls.Add("http://localhost:10002");
                app.Urls.Add("http://localhost:10010");

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

                if (CreateQueues) SetupQueues(app);

                Log.Information($"{Queues.Names.Count} queues added");

                app.Lifetime.ApplicationStarted.Register(() =>
                {
                    using IServiceScope scope = app.Services.CreateScope();
                    IQueueService queueService = scope.ServiceProvider.GetRequiredService<IQueueService>();
                    ITableService tableService = scope.ServiceProvider.GetRequiredService<ITableService>();
                    IBlobService blobService = scope.ServiceProvider.GetRequiredService<IBlobService>();
                    PersistenceSettings persistenceSettings = new();
                    Task.Run(() => Persistence.Restore(persistenceSettings.RootPath.AbsolutePath, queueService, tableService, blobService)).Wait(CancellationToken.None);
                    Log.Information("Persistence restored");
                });

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
                fifoService.CreateQueueAsync(queue);
            }
        }
    }
}