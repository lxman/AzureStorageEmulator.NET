using AzureStorageEmulator.NET.Blob;
using AzureStorageEmulator.NET.Queue;
using AzureStorageEmulator.NET.Queue.Services;
using PeriodicLogger;
using Serilog;
using Serilog.Events;
using SerilogTracing;

// ReSharper disable HeuristicUnreachableCode
#pragma warning disable CS0162 // Unreachable code detected

namespace AzureStorageEmulator.NET
{
    public static class Program
    {
        private static readonly List<string> Queues =
        [
            "accounting-events",
            "accounting-events-2",
            "accounting-inbound",
            "accounting-parallel",
            "accounting-posting",
            "accounting-retry",
            "accounting-retry-2",
            "accounting-sequential",
            "agencypulse-events",
            "agencypulse-events-retry",
            "api-events",
            "api-events-retry",
            "balancerecharge-retry",
            "balancerechargerequest",
            "batchrequests",
            "billing-events",
            "billing-events-retry",
            "bulkactions-batch",
            "bulkactions-bulkimport",
            "bulkactions-bulkimportprocessing",
            "bulkactions-certificateprocessing",
            "bulkactions-dataextraction",
            "bulkactions-documentprocessingtransation",
            "bulkactions-retry",
            "bulkactions-transaction",
            "callrecording",
            "callrecording-deletion",
            "callrecording-storage",
            "campaignscheduler",
            "cimfile",
            "cimfile-large",
            "cimfile-retry",
            "claimsdownload",
            "claimsdownload-retry",
            "claimsreprocess",
            "commissions-processing",
            "commissions-retry",
            "commissions-rules",
            "commissions-serviceteamprocessing",
            "connect",
            "connect-callrecording",
            "connect-outbound",
            "connect-retry",
            "dataconversion-acord-to-ape",
            "dataconversion-applicant-attachment-creation",
            "dataconversion-clientprocessing",
            "dataconversion-completioncheck",
            "dataconversion-document-post-conversion-processing",
            "dataconversion-document-retry",
            "dataconversion-document-upload",
            "dataconversion-email-activity-attachment-upload",
            "dataconversion-initiation",
            "dataconversion-retry",
            "db-tablesync",
            "downloader-jobs",
            "downloader-jobs-highpriority",
            "downloader-jobs-retry",
            "events",
            "events-retry",
            "externalidentity-events",
            "externalidentity-events-retry",
            "externalidentity-litmos-events",
            "ezlynx-bulk-quoting",
            "ezlynx-callrecording-deletion",
            "ezlynx-callrecording-storage",
            "ezlynx-policyworkflow-appsync",
            "ezlynx-policyworkflow-retention",
            "logging-cachemetrics-summaries",
            "logging-dbmetrics-summaries",
            "logging-nontx",
            "logging-retry",
            "notifications-batchevents",
            "notifications-batchevents2",
            "notifications-retry",
            "policyfile",
            "policyfile-large",
            "policyprocessor-retry",
            "policytransaction",
            "policytransaction-large",
            "policyworkflow",
            "policyworkflow-ape",
            "policyworkflow-download-edit",
            "policyworkflow-retry",
            "qasdownload",
            "qasdownload-retry",
            "quoteproposal",
            "rating-dataextraction",
            "rating-dataextraction-retry",
            "rating-policyextraction",
            "rating-service",
            "rating-service-retry",
            "salescenter",
            "salescenter-retry",
            "scheduledmailrequests",
            "scheduling",
            "scheduling-retry",
            "searchindex-documentupdate",
            "searchindex-orgreindexcomplete",
            "searchindex-retry",
            "searchupdate-az-collection1",
            "searchupdate-az-collection1-fullreindex",
            "searchupdate-az-collection1-orgreindex",
            "searchupdate-az-collection1-policy",
            "searchupdate-az-collection1-retry",
            "searchupdate-az-fullreindex",
            "searchupdate-az-notes",
            "searchupdate-az-notes-fullreindex",
            "searchupdate-az-notes-orgreindex",
            "searchupdate-az-notes-retry",
            "searchupdate-az-opportunities",
            "searchupdate-az-opportunities-fullreindex",
            "searchupdate-az-opportunities-orgreindex",
            "searchupdate-az-opportunity-retry",
            "searchupdate-az-orgreindex",
            "searchupdate-retry",
            "submissions",
            "submissions-retry",
            "testq",
            "text-message",
            "text-message-retry",
            "throttler-01",
            "throttler-02",
            "throttler-03",
            "throttler-04",
            "throttler-05",
            "throttler-06",
            "throttler-07",
            "throttler-retry",
            "txscriptexec-complete-batch-01",
            "txscriptexec-complete-batch-02",
            "txscriptexec-complete-batch-03",
            "txscriptexec-complete-batch-04",
            "txscriptexec-complete-batch-05",
            "txscriptexec-complete-batch-06",
            "voip",
            "voip-retry",
            "webhook-events",
            "webhook-events-retry",
            "workflow-dailyprocessevents",
            "workflow-delayedevents",
            "workflow-events",
            "workflow-events-retry",
            "workflow-events-smartstop",
            "workflow-execution-log",
            "workflow-execution-log-ezautomation",
            "workflow-scheduled-actions",
            "workflow-steps-execution",
            "workflow-trackingdata"
        ];

        private const bool DetailedLogging = false;
        private const int BatchSeconds = 2;
        private const int ControllerDelay = 500;
        private const bool LogGetMessages = false;

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
            else
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .WriteTo.PeriodicLoggerSink(BatchSeconds)
                    .CreateLogger();
            }

            Log.Information("Starting up");

            try
            {
                QueueSettings settings = new() { Delay = ControllerDelay, LogGetMessages = LogGetMessages };

                WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

                // Add services to the container.

                builder.Services.AddControllers().AddXmlSerializerFormatters();
                builder.Services.AddSerilog();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSingleton<IFifoService, BlockingCollectionService>();
                builder.Services.AddScoped<IMessageService, MessageService>();
                builder.Services.AddScoped<IBlobService, BlobService>();
                builder.Services.AddSingleton<IQueueSettings>(settings);
                builder.Services.AddCors();

                WebApplication app = builder.Build();

                // Configure the HTTP request pipeline.
                app.UseHttpsRedirection();

                app.UseAuthorization();

                app.MapControllers();

                Log.Information("Adding queues");

                SetupQueues(app);

                Log.Information($"{Queues.Count} queues added");

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
            foreach (string queue in Queues)
            {
                fifoService.AddQueue(queue);
            }
        }
    }
}