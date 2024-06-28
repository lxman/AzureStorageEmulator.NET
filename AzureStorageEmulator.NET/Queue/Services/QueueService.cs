using AzureStorageEmulator.NET.Common;
using AzureStorageEmulator.NET.Queue.Extensions;
using AzureStorageEmulator.NET.Queue.Models;
using AzureStorageEmulator.NET.Queue.Models.MessageResponseLists;
using AzureStorageEmulator.NET.Results;
using AzureStorageEmulator.NET.XmlSerialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Serilog;

// ReSharper disable UnusedVariable
#pragma warning disable IDE0059

namespace AzureStorageEmulator.NET.Queue.Services
{
    public interface IQueueService
    {
        Task<IActionResult> CreateQueueAsync(string queueName, HttpContext context);

        Task<IActionResult> ListQueuesAsync(CancellationToken? cancellationToken, HttpContext context);

        Task<IActionResult> DeleteQueueAsync(string queueName, HttpContext context);

        Task<IActionResult> PutMessageAsync(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context);

        Task<IActionResult> GetMessagesAsync(string queueName, int timeout, HttpContext context);

        Task<IActionResult> GetQueueMetadataAsync(string queueName, HttpContext context);

        Task<IActionResult> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt, int timeout,
            HttpContext context);

        Task<IActionResult> ClearMessagesAsync(string queueName, HttpContext context);

        Task<IActionResult> MessageCountAsync(string queueName, HttpContext context);
    }

    public class QueueService(IFifoService fifoService,
        IXmlSerializer<PutMessageResponseList> putMessageResponseListSerializer,
        IXmlSerializer<PeekMessageResponseList> peekMessageResponseListSerializer,
        IXmlSerializer<GetMessagesResponseList> getMessagesResponseListSerializer,
        IXmlSerializer<QueueEnumerationResults> queueEnumerationResultsSerializer,
        ISettings settings) : IQueueService
    {
        public async Task<IActionResult> CreateQueueAsync(string queueName, HttpContext context)
        {
            Log.Information($"CreateQueue queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            StatusCodeResult result = new(await fifoService.CreateQueueAsync(queueName) ? 201 : 204);
            return result;
        }

        public async Task<IActionResult> ListQueuesAsync(CancellationToken? cancellationToken, HttpContext context)
        {
            Log.Information("ListQueuesAsync");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            if (!queries.TryGetValue("comp", out StringValues values) || !values.Contains("list"))
            {
                return new BadRequestResult();
            }
            QueueEnumerationResults results = new() { ServiceEndpoint = GetBaseUrl(context) };
            if (cancellationToken is { IsCancellationRequested: true }) return new StatusCodeResult(504);
            results.Queues.AddRange(await fifoService.ListQueuesAsync());
            if (cancellationToken is { IsCancellationRequested: true }) return new StatusCodeResult(504);
            results.MaxResults = 5000;
            return new ContentResult
            {
                Content = await queueEnumerationResultsSerializer.Serialize(results),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> DeleteQueueAsync(string queueName, HttpContext context)
        {
            Log.Information($"DeleteQueueAsync name = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            await fifoService.DeleteQueueAsync(queueName);
            return new StatusCodeResult(204);
        }

        public async Task<IActionResult> GetMessagesAsync(string queueName, int timeout, HttpContext context)
        {
            if (settings.QueueSettings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            CancellationTokenSource? cancellationSource = SetupTimeout(timeout);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            (IMethodResult methodResult, List<QueueMessage>? messages) = await fifoService.GetMessagesAsync(queueName,
                queries.TryGetValue("numofmessages", out StringValues numMessagesValue) ? Convert.ToInt32(numMessagesValue.First()) : null,
                queries.TryGetValue("peekonly", out StringValues peekOnlyValue) && Convert.ToBoolean(peekOnlyValue.First()),
                cancellationSource?.Token);
            switch (methodResult)
            {
                case ResultNotFound:
                    return new NotFoundResult();
                case ResultTimeout:
                    return new StatusCodeResult(504);
            }

            if (numMessagesValue.Count > 0 && (Convert.ToInt32(numMessagesValue.First()) > 32 || Convert.ToInt32(numMessagesValue.First()) < 1)) return new BadRequestResult();
            if (peekOnlyValue is { Count: > 0 })
            {
                PeekMessageResponseList peekMessageResponseList = new();
                if (messages is not null) peekMessageResponseList.QueueMessagesList.AddRange(messages.ToPeekMessageResponseList());
                return new ContentResult
                {
                    Content = await peekMessageResponseListSerializer.Serialize(peekMessageResponseList),
                    ContentType = "application/xml",
                    StatusCode = 200
                };
            }
            GetMessagesResponseList getMessagesResponseList = new();
            if (messages is not null)
            {
                messages.ForEach(m => m.InsertionTime = DateTime.UtcNow);
                getMessagesResponseList.QueueMessagesList.AddRange(messages.ToGetMessageResponseList());
            }
            return new ContentResult
            {
                Content = await getMessagesResponseListSerializer.Serialize(getMessagesResponseList),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        public async Task<IActionResult> GetQueueMetadataAsync(string queueName, HttpContext context)
        {
            if (settings.QueueSettings.LogGetMessages) Log.Information($"GetMessagesAsync queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            Models.Queue? result = await fifoService.GetQueueMetadataAsync(queueName);
            if (result is null) return new NotFoundResult();
            context.Response.Headers.Append("x-ms-approximate-messages-count", result.MessageCount.ToString());
            if (result.Metadata is null)
            {
                return new OkResult();
            }

            foreach (Metadata metadata in result.Metadata)
            {
                context.Response.Headers.Append($"x-ms-meta-{metadata.Key}", metadata.Value);
            }
            return new OkResult();
        }

        public async Task<IActionResult> PutMessageAsync(string queueName, QueueMessage message, int visibilityTimeout, int messageTtl, int timeout, HttpContext context)
        {
            Log.Information($"PutMessageAsync queueName = {queueName}, message={message.MessageText}, visibilityTimeout = {visibilityTimeout}, messageTtl = {messageTtl}, timeOut = {timeout}");
            CancellationTokenSource? cancellationSource = SetupTimeout(timeout);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            QueueMessage queueMessage = new()
            {
                DequeueCount = 0,
                TimeToLive = Convert.ToInt32(TimeSpan.FromSeconds(messageTtl).TotalSeconds),
                InsertionTime = DateTime.UtcNow,
                MessageId = Guid.NewGuid(),
                MessageText = message.MessageText,
                PopReceipt = Guid.NewGuid().ToString(),
                VisibilityTimeout = visibilityTimeout
            };
            IMethodResult result = await fifoService.PutMessageAsync(queueName, queueMessage, cancellationSource?.Token);
            switch (result)
            {
                case ResultNotFound resultNotFound:
                    return new NotFoundResult();
                case ResultOk resultOk:
                    PutMessageResponseList putMessageResponseList = new();
                    putMessageResponseList.QueueMessagesList.Add(queueMessage.ToPutMessageResponse());
                    return new ContentResult
                    {
                        Content = await putMessageResponseListSerializer.Serialize(putMessageResponseList),
                        ContentType = "application/xml",
                        StatusCode = 201
                    };
                case ResultTimeout resultTimeout:
                    return new StatusCodeResult(504);
                default:
                    throw new ArgumentOutOfRangeException(result.GetType().Name);
            }
        }

        public async Task<IActionResult> DeleteMessageAsync(string queueName, Guid messageId, string popReceipt,
            int timeout, HttpContext context)
        {
            Log.Information($"DeleteMessageAsync queueName = {queueName}, messageId = {messageId}, popReceipt = {popReceipt}");
            if (string.IsNullOrEmpty(popReceipt)) return new BadRequestResult();
            CancellationTokenSource? cancellationSource = SetupTimeout(timeout == 0 ? int.MaxValue : timeout);
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            (IMethodResult methodResult, QueueMessage? message) = await fifoService.DeleteMessageAsync(queueName, messageId, popReceipt, cancellationSource?.Token);
            return methodResult switch
            {
                ResultOk => new StatusCodeResult(204),
                ResultNotFound => new NotFoundResult(),
                _ => new StatusCodeResult(504)
            };
        }

        public async Task<IActionResult> ClearMessagesAsync(string queueName, HttpContext context)
        {
            Log.Information($"ClearMessagesAsync queueName = {queueName}");
            int result = await fifoService.ClearMessagesAsync(queueName);
            return new StatusCodeResult(result);
        }

        public async Task<IActionResult> MessageCountAsync(string queueName, HttpContext context)
        {
            Log.Information($"MessageCountAsync queueName = {queueName}");
            Dictionary<string, StringValues> queries = QueryProcessor(context.Request);
            return new ContentResult
            {
                Content = (await fifoService.MessageCountAsync(queueName)).ToString(),
                ContentType = "application/xml",
                StatusCode = 200
            };
        }

        private static Dictionary<string, StringValues> QueryProcessor(HttpRequest request)
        {
            return request
                .Query
                .Keys
                .ToDictionary(
                    key => key,
                    key => request
                        .Query
                        .TryGetValue(key, out StringValues values)
                        ? values
                        : []
                );
        }

        private static string GetBaseUrl(HttpContext context) => $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.Path.Value}";

        private static CancellationTokenSource? SetupTimeout(int timeout)
        {
            timeout = timeout > 30 ? 30 : timeout < 0 ? 0 : timeout;
            return timeout > 0 ? new CancellationTokenSource(timeout) : null;
        }
    }
}